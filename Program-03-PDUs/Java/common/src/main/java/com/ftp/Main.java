package com.ftp;

import java.io.InputStream;
import java.io.IOException;
import java.nio.charset.StandardCharsets;

public class Main {
    public static void main(String[] args) {
        System.out.println("Common File !!");
    }
}

interface IPDU {
    public byte getType(); 
}

class PDU {

    public static class Connect implements IPDU {

        @Override
        public byte getType() {
            return 1;
        }

        public String getMessage() {
            return "CONNECT";
        }
    }

    public static class Response implements IPDU {

        private final String message;

        public enum ResponseType {
            ERROR,
            OKAY
        }

        public Response(ResponseType type) {
            this.message = (type == ResponseType.ERROR) ? "ER" : "OK";
        }

        @Override
        public byte getType() {
            return 2;
        }

        public String getMessage() {
            return message;
        }
    }

    public static class Metadata implements IPDU {

        private long fileSize;
        private long fileNameLength;
        private String fileName;

        public Metadata(long fileSize, String fileName) {
            this.fileSize = fileSize;
            this.fileName = fileName;
            this.fileNameLength = (short) fileName.length();
        }

        @Override
        public byte getType() {
            return 3;
        }

        public long getFileSize() {
            return fileSize;
        }

        public void setFileSize(long fileSize) {
            this.fileSize = fileSize;
        }

        public long getFileNameLength() {
            return fileNameLength;
        }

        public void setFileNameLength(long fileNameLength) {
            this.fileNameLength = fileNameLength;
        }

        public String getFileName() {
            return fileName;
        }

        public void setFileName(String fileName) {
            this.fileName = fileName;
        }

        @Override
        public String toString() {
            return "Metadata { fileName: \"" + fileName + "\", fileSize: " + fileSize + " }";
        }

    }

    public static class Data implements IPDU {

        private int payloadSize;
        private byte[] payload;

        public Data(byte[] payload) {
            this.payload = payload;
            this.payloadSize = payload.length;
        }

        @Override
        public byte getType() {
            return 4;
        }

        public int getPayloadSize() {
            return payloadSize;
        }
        
        public void setPayloadSize(int payloadSize) {
            this.payloadSize = payloadSize;
        }

        public byte[] getPayload() {
            return payload;
        }

        public void setPayload(byte[] payload) {
            this.payload = payload;
        }
    }

    public static class Finish implements IPDU {

        @Override
        public byte getType() {
            return 5;
        }

        public String getMessage() {
            return "FINISH";
        }
    }
}

class PDUDeserializer {

    public PDU.Connect deserializeConnect(InputStream stream) throws IOException {
        byte[] buffer = new byte[8];
        int bytesRead = stream.read(buffer, 0, 1);
        if (bytesRead != 1) {
            throw new IOException("Incorrect amount of bytes read.");
        }

        byte type = buffer[0];
        PDU.Connect connectPdu = new PDU.Connect();
        if (type != connectPdu.getType()) {
            throw new IOException("Failed to deserialize CONNECT. Type mismatch.");
        }

        bytesRead = stream.read(buffer, 1, 7);
        if (bytesRead != 7) {
            throw new IOException("Incorrect amount of bytes read for message.");
        }

        String connectMessage = new String(buffer, 1, 7, StandardCharsets.UTF_8);
        if (!connectMessage.equals(connectPdu.getMessage())) {
            throw new IOException("Failed to deserialize CONNECT. Message mismatch.");
        }

        return connectPdu;
    }

    public PDU.Response deserializeResponse(InputStream stream) throws IOException {
        byte[] buffer = new byte[3];
        int bytesRead = stream.read(buffer, 0, 1);
        if (bytesRead != 1) {
            throw new IOException("Incorrect amount of bytes read.");
        }

        byte type = buffer[0];
        if (type != 2) {
            throw new IOException("Failed to deserialize RESPONSE. Type mismatch.");
        }

        bytesRead = stream.read(buffer, 1, 2);
        if (bytesRead != 2) {
            throw new IOException("Incorrect amount of bytes read for response message.");
        }

        String message = new String(buffer, 1, 2, StandardCharsets.UTF_8);
        PDU.Response responsePdu;
        if ("OK".equals(message)) {
            responsePdu = new PDU.Response(PDU.Response.ResponseType.OKAY);
        } else if ("ER".equals(message)) {
            responsePdu = new PDU.Response(PDU.Response.ResponseType.ERROR);
        } else {
            throw new IOException("Unknown RESPONSE message: " + message);
        }

        return responsePdu;
    }

    private byte[] readExact(InputStream stream, int length) throws IOException {
        byte[] data = new byte[length];
        int totalRead = 0;
        while (totalRead < length) {
            int read = stream.read(data, totalRead, length - totalRead);
            if (read == -1) throw new IOException("Unexpected end of stream.");
            totalRead += read;
        }
        return data;
    }

    public PDU.Metadata deserializeMetadata(InputStream stream) throws IOException {
        byte[] buffer = new byte[1];
        int bytesRead = stream.read(buffer);
        if (bytesRead != 1) {
            throw new IOException("Incorrect amount of bytes read.");
        }

        byte type = buffer[0];
        if (type != 3) {
            throw new IOException("Failed to deserialize METADATA. Type mismatch.");
        }

        byte[] fileSizeBytes = readExact(stream, 8);
        long fileSize = bytesToLong(fileSizeBytes);

        byte[] nameLenBytes = readExact(stream, 2);
        short fileNameLength = bytesToShort(nameLenBytes);

        byte[] fileNameBytes = readExact(stream, fileNameLength);
        String fileName = new String(fileNameBytes, StandardCharsets.UTF_8);

        return new PDU.Metadata(fileSize, fileName);
    }

    public PDU.Data deserializeData(InputStream stream) throws IOException {
        byte[] buffer = new byte[1];
        int bytesRead = stream.read(buffer);
        if (bytesRead != 1) {
            throw new IOException("Incorrect amount of bytes read.");
        }

        byte type = buffer[0];
        if (type != 4) {
            throw new IOException("Failed to deserialize DATA. Type mismatch.");
        }

        byte[] sizeBytes = readExact(stream, 4);
        int payloadSize = bytesToInt(sizeBytes);

        byte[] payload = readExact(stream, payloadSize);
        return new PDU.Data(payload);
    }

    private long bytesToLong(byte[] bytes) {
        return ((long)(bytes[0] & 0xFF) << 56) |
               ((long)(bytes[1] & 0xFF) << 48) |
               ((long)(bytes[2] & 0xFF) << 40) |
               ((long)(bytes[3] & 0xFF) << 32) |
               ((long)(bytes[4] & 0xFF) << 24) |
               ((long)(bytes[5] & 0xFF) << 16) |
               ((long)(bytes[6] & 0xFF) << 8) |
               ((long)(bytes[7] & 0xFF));
    }

    private int bytesToInt(byte[] bytes) {
        return ((bytes[0] & 0xFF) << 24) |
               ((bytes[1] & 0xFF) << 16) |
               ((bytes[2] & 0xFF) << 8) |
               (bytes[3] & 0xFF);
    }

    private short bytesToShort(byte[] bytes) {
        return (short)(((bytes[0] & 0xFF) << 8) |
                        (bytes[1] & 0xFF));
    }
}

class PDUSerializer {

    public byte[] serializeConnect(PDU.Connect connect) {
        byte[] messageBytes = connect.getMessage().getBytes(StandardCharsets.UTF_8);
        byte[] pdu = new byte[1 + messageBytes.length];
        pdu[0] = connect.getType();
        System.arraycopy(messageBytes, 0, pdu, 1, messageBytes.length);
        return pdu;
    }

    public byte[] serializeResponse(PDU.Response response) {
        byte[] messageBytes = response.getMessage().getBytes(StandardCharsets.UTF_8);
        byte[] pdu = new byte[1 + messageBytes.length];
        pdu[0] = response.getType();
        System.arraycopy(messageBytes, 0, pdu, 1, messageBytes.length);
        return pdu;
    }

    public byte[] serializeMetadata(PDU.Metadata metadata) {
        byte[] fileSizeBytes = longToBytes(metadata.getFileSize());
        byte[] fileNameLengthBytes = shortToBytes(metadata.getFileNameLength());
        byte[] fileNameBytes  = metadata.getFileName().getBytes(StandardCharsets.UTF_8);

        int totalSize = 1 + fileSizeBytes.length + fileNameLengthBytes.length + fileNameBytes.length;
        byte[] pdu = new byte[totalSize];

        int offset = 0;
        pdu[offset++] = metadata.getType();

        System.arraycopy(fileSizeBytes, 0, pdu , offset, fileSizeBytes.length);
        offset += fileSizeBytes.length;

        System.arraycopy(fileNameLengthBytes, 0, pdu, offset, fileNameLengthBytes.length);
        offset += fileNameLengthBytes.length;

        System.arraycopy(fileNameBytes, 0, pdu, offset, fileNameBytes.length);

        return pdu;
    }

    public byte[] serializeData(PDU.Data data) {
        byte[] payloadSizeBytes = intToBytes(data.getPayloadSize());
        byte[] payload = data.getPayload();

        int totalSize = 1 + payloadSizeBytes.length + payload.length;
        byte[] pdu = new byte[totalSize];

        int offset = 0;
        pdu[offset++] = data.getType();

        System.arraycopy(payloadSizeBytes, 0, pdu, offset, payloadSizeBytes.length);
        offset += payloadSizeBytes.length;

        System.arraycopy(payload, 0, pdu, offset, payload.length);
        return pdu;
    }

    private byte[] longToBytes(long value) {
        return new byte[] {
            (byte)(value >>> 56),
            (byte)(value >>> 48),
            (byte)(value >>> 40),
            (byte)(value >>> 32),
            (byte)(value >>> 24),
            (byte)(value >>> 16),
            (byte)(value >>> 8),
            (byte)value
        };
    }

    private byte[] intToBytes(long value) {
        return new byte[] {
            (byte)(value >>> 24),
            (byte)(value >>> 16),
            (byte)(value >>> 8),
            (byte)value
        };
    }

    private byte[] shortToBytes(long value) {
        return new byte[] {
            (byte)(value >>> 8),
            (byte)value
        };
    }
}


