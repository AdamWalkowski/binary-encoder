# #1 Simple binary encoding for messages

To properly decode message from binary stream some structured informations is needed to avoid traversing through the streams in search for some special bytes dividing blocks of data.

## #1.1 Assumptions about model:

1. A message can contain a binary payload limited to 256 kB.
2. A message can contain maximal 63 headers as pairs of ASCII-encoded strings.
3. Each header lenght (name + value) is limited to 2046 bytes.

## #1.2 Assumptions about encoding algorithm:

1. Should not be generic rather optimized to specific structure described in #1.1.
2. Optimization for transport (data compression) is not a requirement.
3. Should be simple and easy to maintain and present accepting lower encoding/decoding performance (avoiding unmanaged code).

## #1.3 Therefore simple binary stream structure should contain:

1. Headers count.
2. For each header offset between name part and value part.
3. Optional checksum (hash).

# #2 Binary stream encoding

Message model contains a dictionary of headers and raw byte stream of message payload. For effectivness of decoding I decided to place in encoded stream
metadata information about:

1. payload offest - 4 bytes,
2. number of headers - 2 bytes,
3. for each header a 4 byte prefix - first two bytes stores offset of the value part, second 2 bytes stores length of value part.

| payload offset | 4 bytes
| headers count | 2 bytes
| value1 offset | 2 bytes
| value1 length | 2 bytes
| name1 | 1 kb
| value1 | 1 kb
| value2 offset | 2 bytes
| value2 length | 2 bytes
| name2 | 1 kb
| value2 | 1 kb
| ... | ...
| payload | 256 kb

Metadata are introduced to make decoding efficient and avoid scanning whole streams for special bytes etc.
Payload offset on start tells what is the volume of headers part of the encoded stream.
Value offset simplifies reading name part of heas whereas value length simplifies reading of value part.

# #3 Implementation

To keep codec structure simple and clean I recognized two aspects of the process - encoding sheme layer and binary operations layer (mapping primitives into bytes streams).
Each aspect is encapsulated in separate module - MessageCodec and ByteBufferBuilder respectively. ByteBufferBuilder module is a ependency of MassageCodec module.

I've started from pure unit tests for an algorithm as specification for my coding algorithm. Also included tests for binary streams builder and
two integration tests for decoding.

Accoringly to task description I assume that headers and payload are not obligatory for message (...can contain...) and I assume that
completely empty message (without and headers and no payload) is invalid.
