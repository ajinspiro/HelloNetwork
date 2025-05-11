package com.ftp;

import java.io.*;
import java.net.*;
import java.nio.file.*;
import java.util.Scanner;

public class Server {

    public static void main(String[] args) throws Exception {
        System.out.println("SERVER\n");

        Scanner scanner = new Scanner(System.in);

        System.out.print("Enter server IP address: ");
        InetAddress serverIP = InetAddress.getByName(scanner.nextLine());

        System.out.print("Enter server port: ");
        int serverPort = Integer.parseInt(scanner.nextLine());
        
        scanner.close();

        PDUSerializer pduSerializer = new PDUSerializer();
        PDUDeserializer pduDeserializer = new PDUDeserializer();

        try (ServerSocket serverSocket = new ServerSocket(serverPort, 50, serverIP)) {
            while (true) {
                Socket clientSocket = serverSocket.accept();
                new Thread(() -> {
                    try {
                        processClient(clientSocket, pduSerializer, pduDeserializer);
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                }).start();
            }
        }
    }

    private static void processClient(Socket clientSocket, PDUSerializer pduSerializer, PDUDeserializer pduDeserializer) throws IOException {
        try (InputStream in = clientSocket.getInputStream();
             OutputStream out = clientSocket.getOutputStream()) {

            pduDeserializer.deserializeConnect(in);
            System.out.println("CONNECT received.\nSending RESPONSE(OK)...");

            byte[] responsePduBytes = pduSerializer.serializeResponse(new PDU.Response(PDU.Response.ResponseType.OKAY));
            out.write(responsePduBytes);
            System.out.println("Complete");

            PDU.Metadata metadataPDU = pduDeserializer.deserializeMetadata(in);
            System.out.println("METADATA received. " + metadataPDU);

            long availableFreeSpace = Files.getFileStore(Paths.get(".")).getUsableSpace();

            PDU.Response.ResponseType responseType = availableFreeSpace <= metadataPDU.getFileSize()
                    ? PDU.Response.ResponseType.ERROR
                    : PDU.Response.ResponseType.OKAY;

            PDU.Response metadataResponse = new PDU.Response(responseType);
            System.out.print("Sending RESPONSE(" + metadataResponse.getMessage() + ")...");
            out.write(pduSerializer.serializeResponse(metadataResponse));
            System.out.println("Complete");

            if (responseType == PDU.Response.ResponseType.ERROR) {
                System.out.println("Not enough space to receive the file. Exiting.");
                return;
            }

            int totalBytesRead = 0;
            ByteArrayOutputStream payloadBuffer = new ByteArrayOutputStream();

            short packetNumber = 1;
            while (totalBytesRead < metadataPDU.getFileSize()) {
                PDU.Data dataPDU = pduDeserializer.deserializeData(in);
                System.out.println("DATA(Packet number=" + packetNumber++ + ") received.");
                System.out.print("Sending RESPONSE(OK)...");
                out.write(responsePduBytes);
                System.out.println("Completed");
                totalBytesRead += dataPDU.getPayloadSize();
                payloadBuffer.write(dataPDU.getPayload());
            }

            try (FileOutputStream fos = new FileOutputStream(metadataPDU.getFileName())) {
                payloadBuffer.writeTo(fos);
                fos.flush();
            }

            System.out.println("Transfer complete.");
        } catch (Exception e) {
            System.err.println("Processing failed. Exception: " + e.getClass().getName() + " Message: " + e.getMessage());
        }
    }
}
