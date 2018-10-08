// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: ClassModel.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Wist.Proto.Model {

  /// <summary>Holder for reflection information generated from ClassModel.proto</summary>
  public static partial class ClassModelReflection {

    #region Descriptor
    /// <summary>File descriptor for ClassModel.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ClassModelReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChBDbGFzc01vZGVsLnByb3RvEhBXaXN0LlByb3RvLk1vZGVsIgcKBUVtcHR5",
            "Ii0KDUxhc3RTeW5jQmxvY2sSDgoGSGVpZ2h0GAEgASgEEgwKBEhhc2gYAiAB",
            "KAwiLgoZVHJhbnNhY3Rpb25hbEJsb2NrUmVxdWVzdBIRCglQdWJsaWNLZXkY",
            "ASABKAwiUAoZVHJhbnNhY3Rpb25hbEJsb2NrRXNzZW5zZRIOCgZIZWlnaHQY",
            "ASABKAQSDAoESGFzaBgCIAEoDBIVCg1VcFRvRGF0ZUZ1bmRzGAMgASgEMl0K",
            "C1N5bmNNYW5hZ2VyEk4KEEdldExhc3RTeW5jQmxvY2sSFy5XaXN0LlByb3Rv",
            "Lk1vZGVsLkVtcHR5Gh8uV2lzdC5Qcm90by5Nb2RlbC5MYXN0U3luY0Jsb2Nr",
            "IgAylAEKGVRyYW5zYWN0aW9uYWxDaGFpbk1hbmFnZXISdwoZR2V0TGFzdFRy",
            "YW5zYWN0aW9uYWxCbG9jaxIrLldpc3QuUHJvdG8uTW9kZWwuVHJhbnNhY3Rp",
            "b25hbEJsb2NrUmVxdWVzdBorLldpc3QuUHJvdG8uTW9kZWwuVHJhbnNhY3Rp",
            "b25hbEJsb2NrRXNzZW5zZSIAYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Wist.Proto.Model.Empty), global::Wist.Proto.Model.Empty.Parser, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Wist.Proto.Model.LastSyncBlock), global::Wist.Proto.Model.LastSyncBlock.Parser, new[]{ "Height", "Hash" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Wist.Proto.Model.TransactionalBlockRequest), global::Wist.Proto.Model.TransactionalBlockRequest.Parser, new[]{ "PublicKey" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Wist.Proto.Model.TransactionalBlockEssense), global::Wist.Proto.Model.TransactionalBlockEssense.Parser, new[]{ "Height", "Hash", "UpToDateFunds" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class Empty : pb::IMessage<Empty> {
    private static readonly pb::MessageParser<Empty> _parser = new pb::MessageParser<Empty>(() => new Empty());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Empty> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Wist.Proto.Model.ClassModelReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Empty() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Empty(Empty other) : this() {
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Empty Clone() {
      return new Empty(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Empty);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Empty other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Empty other) {
      if (other == null) {
        return;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
        }
      }
    }

  }

  public sealed partial class LastSyncBlock : pb::IMessage<LastSyncBlock> {
    private static readonly pb::MessageParser<LastSyncBlock> _parser = new pb::MessageParser<LastSyncBlock>(() => new LastSyncBlock());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<LastSyncBlock> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Wist.Proto.Model.ClassModelReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public LastSyncBlock() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public LastSyncBlock(LastSyncBlock other) : this() {
      height_ = other.height_;
      hash_ = other.hash_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public LastSyncBlock Clone() {
      return new LastSyncBlock(this);
    }

    /// <summary>Field number for the "Height" field.</summary>
    public const int HeightFieldNumber = 1;
    private ulong height_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ulong Height {
      get { return height_; }
      set {
        height_ = value;
      }
    }

    /// <summary>Field number for the "Hash" field.</summary>
    public const int HashFieldNumber = 2;
    private pb::ByteString hash_ = pb::ByteString.Empty;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pb::ByteString Hash {
      get { return hash_; }
      set {
        hash_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as LastSyncBlock);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(LastSyncBlock other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Height != other.Height) return false;
      if (Hash != other.Hash) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Height != 0UL) hash ^= Height.GetHashCode();
      if (Hash.Length != 0) hash ^= Hash.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Height != 0UL) {
        output.WriteRawTag(8);
        output.WriteUInt64(Height);
      }
      if (Hash.Length != 0) {
        output.WriteRawTag(18);
        output.WriteBytes(Hash);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Height != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(Height);
      }
      if (Hash.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeBytesSize(Hash);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(LastSyncBlock other) {
      if (other == null) {
        return;
      }
      if (other.Height != 0UL) {
        Height = other.Height;
      }
      if (other.Hash.Length != 0) {
        Hash = other.Hash;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Height = input.ReadUInt64();
            break;
          }
          case 18: {
            Hash = input.ReadBytes();
            break;
          }
        }
      }
    }

  }

  public sealed partial class TransactionalBlockRequest : pb::IMessage<TransactionalBlockRequest> {
    private static readonly pb::MessageParser<TransactionalBlockRequest> _parser = new pb::MessageParser<TransactionalBlockRequest>(() => new TransactionalBlockRequest());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<TransactionalBlockRequest> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Wist.Proto.Model.ClassModelReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TransactionalBlockRequest() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TransactionalBlockRequest(TransactionalBlockRequest other) : this() {
      publicKey_ = other.publicKey_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TransactionalBlockRequest Clone() {
      return new TransactionalBlockRequest(this);
    }

    /// <summary>Field number for the "PublicKey" field.</summary>
    public const int PublicKeyFieldNumber = 1;
    private pb::ByteString publicKey_ = pb::ByteString.Empty;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pb::ByteString PublicKey {
      get { return publicKey_; }
      set {
        publicKey_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as TransactionalBlockRequest);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(TransactionalBlockRequest other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (PublicKey != other.PublicKey) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (PublicKey.Length != 0) hash ^= PublicKey.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (PublicKey.Length != 0) {
        output.WriteRawTag(10);
        output.WriteBytes(PublicKey);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (PublicKey.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeBytesSize(PublicKey);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(TransactionalBlockRequest other) {
      if (other == null) {
        return;
      }
      if (other.PublicKey.Length != 0) {
        PublicKey = other.PublicKey;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            PublicKey = input.ReadBytes();
            break;
          }
        }
      }
    }

  }

  public sealed partial class TransactionalBlockEssense : pb::IMessage<TransactionalBlockEssense> {
    private static readonly pb::MessageParser<TransactionalBlockEssense> _parser = new pb::MessageParser<TransactionalBlockEssense>(() => new TransactionalBlockEssense());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<TransactionalBlockEssense> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Wist.Proto.Model.ClassModelReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TransactionalBlockEssense() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TransactionalBlockEssense(TransactionalBlockEssense other) : this() {
      height_ = other.height_;
      hash_ = other.hash_;
      upToDateFunds_ = other.upToDateFunds_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TransactionalBlockEssense Clone() {
      return new TransactionalBlockEssense(this);
    }

    /// <summary>Field number for the "Height" field.</summary>
    public const int HeightFieldNumber = 1;
    private ulong height_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ulong Height {
      get { return height_; }
      set {
        height_ = value;
      }
    }

    /// <summary>Field number for the "Hash" field.</summary>
    public const int HashFieldNumber = 2;
    private pb::ByteString hash_ = pb::ByteString.Empty;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pb::ByteString Hash {
      get { return hash_; }
      set {
        hash_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "UpToDateFunds" field.</summary>
    public const int UpToDateFundsFieldNumber = 3;
    private ulong upToDateFunds_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ulong UpToDateFunds {
      get { return upToDateFunds_; }
      set {
        upToDateFunds_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as TransactionalBlockEssense);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(TransactionalBlockEssense other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Height != other.Height) return false;
      if (Hash != other.Hash) return false;
      if (UpToDateFunds != other.UpToDateFunds) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Height != 0UL) hash ^= Height.GetHashCode();
      if (Hash.Length != 0) hash ^= Hash.GetHashCode();
      if (UpToDateFunds != 0UL) hash ^= UpToDateFunds.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Height != 0UL) {
        output.WriteRawTag(8);
        output.WriteUInt64(Height);
      }
      if (Hash.Length != 0) {
        output.WriteRawTag(18);
        output.WriteBytes(Hash);
      }
      if (UpToDateFunds != 0UL) {
        output.WriteRawTag(24);
        output.WriteUInt64(UpToDateFunds);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Height != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(Height);
      }
      if (Hash.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeBytesSize(Hash);
      }
      if (UpToDateFunds != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(UpToDateFunds);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(TransactionalBlockEssense other) {
      if (other == null) {
        return;
      }
      if (other.Height != 0UL) {
        Height = other.Height;
      }
      if (other.Hash.Length != 0) {
        Hash = other.Hash;
      }
      if (other.UpToDateFunds != 0UL) {
        UpToDateFunds = other.UpToDateFunds;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Height = input.ReadUInt64();
            break;
          }
          case 18: {
            Hash = input.ReadBytes();
            break;
          }
          case 24: {
            UpToDateFunds = input.ReadUInt64();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code