﻿/*
Technitium Library
Copyright (C) 2021  Shreyas Zare (shreyas@technitium.com)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using TechnitiumLibrary.IO;

namespace TechnitiumLibrary.Net.Dns.ResourceRecords
{
    public enum DnssecNSEC3HashAlgorithm : byte
    {
        Unknown = 0,
        SHA1 = 1
    }

    [Flags]
    public enum DnssecNSEC3Flags : byte
    {
        None = 0,
        OptOut = 1
    }

    //DNS Security (DNSSEC) Hashed Authenticated Denial of Existence
    //https://datatracker.ietf.org/doc/html/rfc5155

    //Authenticated Denial of Existence in the DNS 
    //https://datatracker.ietf.org/doc/html/rfc7129

    public class DnsNSEC3Record : DnsResourceRecordData
    {
        #region variables

        DnssecNSEC3HashAlgorithm _hashAlgorithm;
        DnssecNSEC3Flags _flags;
        ushort _iterations;
        byte[] _salt;
        byte[] _nextHashedOwnerNameValue;
        IReadOnlyList<DnsResourceRecordType> _types;

        string _nextHashedOwnerName;
        bool _isInsecureDelegation;
        bool _isAncestorDelegation;

        byte[] _rData;

        #endregion

        #region constructors

        public DnsNSEC3Record(DnssecNSEC3HashAlgorithm hashAlgorithm, DnssecNSEC3Flags flags, ushort iterations, byte[] salt, byte[] nextHashedOwnerName, IReadOnlyList<DnsResourceRecordType> types)
        {
            _hashAlgorithm = hashAlgorithm;
            _flags = flags;
            _iterations = iterations;
            _salt = salt;
            _nextHashedOwnerNameValue = nextHashedOwnerName;
            _types = types;

            Serialize();
        }

        public DnsNSEC3Record(Stream s)
            : base(s)
        { }

        public DnsNSEC3Record(dynamic jsonResourceRecord)
        {
            _rdLength = Convert.ToUInt16(jsonResourceRecord.data.Value.Length);

            string[] parts = (jsonResourceRecord.data.Value as string).Split(' ');

            _hashAlgorithm = Enum.Parse<DnssecNSEC3HashAlgorithm>(parts[0], true);
            _flags = Enum.Parse<DnssecNSEC3Flags>(parts[1], true);
            _iterations = ushort.Parse(parts[2]);
            _salt = parts[3] == "-" ? Array.Empty<byte>() : Convert.FromHexString(parts[3]);
            _nextHashedOwnerNameValue = Base32.FromBase32HexString(parts[4]);

            DnsResourceRecordType[] types = new DnsResourceRecordType[parts.Length - 5];

            for (int i = 0; i < types.Length; i++)
                types[i] = Enum.Parse<DnsResourceRecordType>(parts[i + 5], true);

            _types = types;

            Serialize();
        }

        #endregion

        #region static

        public static DnssecProofOfNonExistence GetValidatedProofOfNonExistence(IReadOnlyList<DnsResourceRecord> nsec3Records, string domain, DnsResourceRecordType type, bool wildcardAnswerValidation)
        {
            //find proof for closest encloser
            string closestEncloser;
            string nextCloserName;
            bool foundClosestEncloserProof;

            if (wildcardAnswerValidation)
            {
                //wildcard answer case
                closestEncloser = GetParentZone(domain);
                nextCloserName = domain;
                foundClosestEncloserProof = true;
            }
            else
            {
                closestEncloser = domain;
                nextCloserName = null;
                foundClosestEncloserProof = false;
            }

            while (!foundClosestEncloserProof)
            {
                string hashedClosestEncloser = null;

                foreach (DnsResourceRecord record in nsec3Records)
                {
                    if (record.Type != DnsResourceRecordType.NSEC3)
                        continue;

                    DnsNSEC3Record nsec3 = record.RDATA as DnsNSEC3Record;
                    string hashedOwnerName = GetHashedOwnerName(record.Name);

                    if (hashedClosestEncloser is null)
                        hashedClosestEncloser = nsec3.ComputeHashedOwnerName(closestEncloser);

                    if (hashedOwnerName.Equals(hashedClosestEncloser, StringComparison.OrdinalIgnoreCase))
                    {
                        //found proof for closest encloser

                        if (closestEncloser.Equals(domain, StringComparison.OrdinalIgnoreCase))
                        {
                            //domain matches exactly with closest encloser

                            //check if the NSEC3 is an "ancestor delegation"
                            if ((type != DnsResourceRecordType.DS) && nsec3._isAncestorDelegation)
                                continue; //cannot prove with ancestor delegation NSEC3; try next NSEC3

                            return nsec3.GetProofOfNonExistenceFromRecordTypes(type);
                        }

                        foundClosestEncloserProof = true;
                        break;
                    }
                }

                if (foundClosestEncloserProof)
                    break;

                nextCloserName = closestEncloser;
                closestEncloser = GetParentZone(closestEncloser);
                if (closestEncloser is null)
                    return DnssecProofOfNonExistence.NoProof; //could not find any proof
            }

            if (closestEncloser.Length > 0)
                closestEncloser = "." + closestEncloser;

            //find proof for next closer name
            bool foundNextCloserNameProof = false;
            string hashedNextCloserName = null;

            foreach (DnsResourceRecord record in nsec3Records)
            {
                if (record.Type != DnsResourceRecordType.NSEC3)
                    continue;

                DnsNSEC3Record nsec3 = record.RDATA as DnsNSEC3Record;
                string hashedOwnerName = GetHashedOwnerName(record.Name);

                if (hashedNextCloserName is null)
                    hashedNextCloserName = nsec3.ComputeHashedOwnerName(nextCloserName);

                if (DnsNSECRecord.IsDomainCovered(hashedOwnerName, nsec3._nextHashedOwnerName, hashedNextCloserName))
                {
                    //found proof of cover for next closer name

                    //check if the NSEC3 is an "ancestor delegation"
                    if ((type != DnsResourceRecordType.DS) && nsec3._isAncestorDelegation && domain.EndsWith("." + nextCloserName, StringComparison.OrdinalIgnoreCase))
                        continue; //cannot prove with ancestor delegation NSEC3; try next NSEC3

                    if (nsec3._flags.HasFlag(DnssecNSEC3Flags.OptOut))
                        return DnssecProofOfNonExistence.OptOut;

                    foundNextCloserNameProof = true;
                    break;
                }
            }

            if (!foundNextCloserNameProof)
                return DnssecProofOfNonExistence.NoProof;

            //found next closer name proof; so the domain does not exists but there could be a possibility of wildcard that may exist which also needs to be proved as non-existent

            //find proof for wildcard NXDomain
            string wildcardDomain = "*" + closestEncloser;
            string hashedWildcardDomainName = null;

            foreach (DnsResourceRecord record in nsec3Records)
            {
                if (record.Type != DnsResourceRecordType.NSEC3)
                    continue;

                DnsNSEC3Record nsec3 = record.RDATA as DnsNSEC3Record;
                string hashedOwnerName = GetHashedOwnerName(record.Name);

                if (hashedWildcardDomainName is null)
                    hashedWildcardDomainName = nsec3.ComputeHashedOwnerName(wildcardDomain);

                if (hashedOwnerName.Equals(hashedWildcardDomainName, StringComparison.OrdinalIgnoreCase))
                {
                    //found proof for wildcard domain

                    //check if the NSEC3 is an "ancestor delegation"
                    if ((type != DnsResourceRecordType.DS) && nsec3._isAncestorDelegation)
                        continue; //cannot prove with ancestor delegation NSEC3; try next NSEC3

                    //response failed to prove that the domain does not exists since a wildcard exists
                    return DnssecProofOfNonExistence.NoProof;
                }
                else if (DnsNSECRecord.IsDomainCovered(hashedOwnerName, nsec3._nextHashedOwnerName, hashedWildcardDomainName))
                {
                    //found proof of cover for wildcard domain

                    //check if the NSEC3 is an "ancestor delegation"
                    if ((type != DnsResourceRecordType.DS) && nsec3._isAncestorDelegation && domain.EndsWith("." + closestEncloser, StringComparison.OrdinalIgnoreCase))
                        continue; //cannot prove with ancestor delegation NSEC3; try next NSEC3

                    if (nsec3._flags.HasFlag(DnssecNSEC3Flags.OptOut))
                    {
                        //there is opt-out so there could be a wildcard domain
                        //response failed to prove that the domain does not exists since a wildcard MAY exists
                        return DnssecProofOfNonExistence.NoProof;
                    }
                    else
                    {
                        //no opt-out so wildcard domain does not exists
                        //proved that the actual domain does not exists since a wildcard does not exists
                        return DnssecProofOfNonExistence.NxDomain;
                    }
                }
            }

            //found no proof
            return DnssecProofOfNonExistence.NoProof;
        }

        #endregion

        #region private

        private DnssecProofOfNonExistence GetProofOfNonExistenceFromRecordTypes(DnsResourceRecordType checkType)
        {
            if ((checkType == DnsResourceRecordType.DS) && _isInsecureDelegation)
                return DnssecProofOfNonExistence.InsecureDelegation;

            //find if record set exists
            foreach (DnsResourceRecordType type in _types)
            {
                if ((type == checkType) || (type == DnsResourceRecordType.CNAME))
                    return DnssecProofOfNonExistence.RecordSetExists;
            }

            //found no record set
            return DnssecProofOfNonExistence.NoData;
        }

        private static string GetHashedOwnerName(string domain)
        {
            int i = domain.IndexOf('.');
            if (i < 0)
                return domain;

            return domain.Substring(0, i);
        }

        private static string GetParentZone(string domain)
        {
            if (domain.Length == 0)
                return null; //no parent for root

            int i = domain.IndexOf('.');
            if (i > -1)
                return domain.Substring(i + 1);

            //return root zone
            return string.Empty;
        }

        private static byte[] ComputeHashedOwnerName(string ownerName, DnssecNSEC3HashAlgorithm hashAlgorithm, ushort iterations, byte[] salt)
        {
            HashAlgorithm hash;

            switch (hashAlgorithm)
            {
                case DnssecNSEC3HashAlgorithm.SHA1:
                    hash = SHA1.Create();
                    break;

                default:
                    throw new NotSupportedException("NSEC3 hash algorithm is not supported: " + hashAlgorithm.ToString());
            }

            byte[] x;

            using (hash)
            {
                using (MemoryStream mS = new MemoryStream(Math.Max(ownerName.Length, hash.HashSize / 8)))
                {
                    DnsDatagram.SerializeDomainName(ownerName.ToLower(), mS);
                    mS.Write(salt);

                    mS.Position = 0;
                    x = hash.ComputeHash(mS);

                    for (int i = 0; i < iterations; i++)
                    {
                        mS.SetLength(0);

                        mS.Write(x);
                        mS.Write(salt);

                        mS.Position = 0;
                        x = hash.ComputeHash(mS);
                    }
                }
            }

            return x;
        }

        private void Serialize()
        {
            using (MemoryStream mS = new MemoryStream())
            {
                mS.WriteByte((byte)_hashAlgorithm);
                mS.WriteByte((byte)_flags);
                DnsDatagram.WriteUInt16NetworkOrder(_iterations, mS);
                mS.WriteByte((byte)_salt.Length);
                mS.Write(_salt);
                mS.WriteByte((byte)_nextHashedOwnerNameValue.Length);
                mS.Write(_nextHashedOwnerNameValue);
                DnsNSECRecord.WriteTypeBitMapsTo(_types, mS);

                _rData = mS.ToArray();
            }

            _nextHashedOwnerName = Base32.ToBase32HexString(_nextHashedOwnerNameValue).ToLower();

            CheckForDelegation();
        }

        private void CheckForDelegation()
        {
            bool foundDS = false;
            bool foundSOA = false;
            bool foundNS = false;
            bool foundDNAME = false;

            foreach (DnsResourceRecordType type in _types)
            {
                switch (type)
                {
                    case DnsResourceRecordType.DS:
                        foundDS = true;
                        break;

                    case DnsResourceRecordType.SOA:
                        foundSOA = true;
                        break;

                    case DnsResourceRecordType.NS:
                        foundNS = true;
                        break;

                    case DnsResourceRecordType.DNAME:
                        foundDNAME = true;
                        break;
                }
            }

            _isInsecureDelegation = !foundDS && !foundSOA && foundNS;
            _isAncestorDelegation = (foundNS && !foundSOA) || foundDNAME;
        }

        #endregion

        #region protected

        protected override void ReadRecordData(Stream s)
        {
            _rData = s.ReadBytes(_rdLength);

            using (MemoryStream mS = new MemoryStream(_rData))
            {
                _hashAlgorithm = (DnssecNSEC3HashAlgorithm)mS.ReadByteValue();
                _flags = (DnssecNSEC3Flags)mS.ReadByteValue();
                _iterations = DnsDatagram.ReadUInt16NetworkOrder(mS);
                _salt = mS.ReadBytes(mS.ReadByteValue());
                _nextHashedOwnerNameValue = mS.ReadBytes(mS.ReadByteValue());
                _types = DnsNSECRecord.ReadTypeBitMapsFrom(mS, (int)(mS.Length - mS.Position));
            }

            _nextHashedOwnerName = Base32.ToBase32HexString(_nextHashedOwnerNameValue).ToLower();

            CheckForDelegation();
        }

        protected override void WriteRecordData(Stream s, List<DnsDomainOffset> domainEntries, bool canonicalForm)
        {
            s.Write(_rData);
        }

        #endregion

        #region public

        public string ComputeHashedOwnerName(string ownerName)
        {
            return Base32.ToBase32HexString(ComputeHashedOwnerName(ownerName, _hashAlgorithm, _iterations, _salt)).ToLower();
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj is DnsNSEC3Record other)
            {
                if (_hashAlgorithm != other._hashAlgorithm)
                    return false;

                if (_flags != other._flags)
                    return false;

                if (_iterations != other._iterations)
                    return false;

                if (!BinaryNumber.Equals(_salt, other._salt))
                    return false;

                if (!BinaryNumber.Equals(_nextHashedOwnerNameValue, other._nextHashedOwnerNameValue))
                    return false;

                if (_types.Count != other._types.Count)
                    return false;

                for (int i = 0; i < _types.Count; i++)
                {
                    if (_types[i] != other._types[i])
                        return false;
                }

                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_hashAlgorithm, _flags, _iterations, _salt, _nextHashedOwnerNameValue, _types);
        }

        public override string ToString()
        {
            string str = (byte)_hashAlgorithm + " " + (byte)_flags + " " + _iterations + " " + (_salt.Length == 0 ? "-" : Convert.ToHexString(_salt)) + " " + Base32.ToBase32HexString(_nextHashedOwnerNameValue);

            foreach (DnsResourceRecordType type in _types)
                str += " " + type.ToString();

            return str;
        }

        #endregion

        #region properties

        public DnssecNSEC3HashAlgorithm HashAlgorithm
        { get { return _hashAlgorithm; } }

        public DnssecNSEC3Flags Flags
        { get { return _flags; } }

        public ushort Iterations
        { get { return _iterations; } }

        public string Salt
        { get { return Convert.ToHexString(_salt); } }

        [IgnoreDataMember]
        public byte[] SaltValue
        { get { return _salt; } }

        public string NextHashedOwnerName
        { get { return _nextHashedOwnerName; } }

        [IgnoreDataMember]
        public byte[] NextHashedOwnerNameValue
        { get { return _nextHashedOwnerNameValue; } }

        public IReadOnlyList<DnsResourceRecordType> Types
        { get { return _types; } }

        [IgnoreDataMember]
        public override ushort UncompressedLength
        { get { return Convert.ToUInt16(_rData.Length); } }

        #endregion
    }
}