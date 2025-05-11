package com.ftp;

import java.io.*;
import java.net.*;
import java.nio.file.*;
import java.util.Arrays;
import java.util.Scanner;

public class Client {
    public static void main(String[] args) throws Exception {
        System.out.println("CLIENT\n");

        Scanner scanner = new Scanner(System.in);

        System.out.print("Enter server IP address: ");
        InetAddress serverIP = InetAddress.getByName(scanner.nextLine());

        System.out.print("Enter server port: ");
        int serverPort = Integer.parseInt(scanner.nextLine());

        System.out.print("Enter full file path to send: ");
        String payloadFilePath = scanner.nextLine();
        Path path = Paths.get(payloadFilePath);

        if (!Files.exists(path)) {
            System.err.println("File not found: " + payloadFilePath);
            System.exit(-1);
        }

        Thread.sleep(1000);

        try (Socket socket = new Socket(serverIP, serverPort);
            InputStream in = socket.getInputStream();
            OutputStream out = socket.getOutputStream();
            FileInputStream payloadStream = new FileInputStream(payloadFilePath)) {

            PDUSerializer pduSerializer = new PDUSerializer();
            PDUDeserializer pduDeserializer = new PDUDeserializer();

            byte[] connectPDUBytes = pduSerializer.serializeConnect(new PDU.Connect());
            System.out.print("Sending CONNECT...");
            out.write(connectPDUBytes);
            System.out.println("Complete");

            PDU.Response responsePDU = pduDeserializer.deserializeResponse(in);
            System.out.println("RESPONSE(" + responsePDU.getMessage() + ") received.");
            if (responsePDU.getMessage().equals("ER")) {
                System.out.println("Exiting because of server error.");
                System.exit(-1);
            }

            System.out.print("Sending METADATA...");
            PDU.Metadata metadataPDU = new PDU.Metadata(payloadStream.getChannel().size(), Paths.get(payloadFilePath).getFileName().toString());
            byte[] metadataPDUBytes = pduSerializer.serializeMetadata(metadataPDU);
            out.write(metadataPDUBytes);
            System.out.println("Complete");

            responsePDU = pduDeserializer.deserializeResponse(in);
            System.out.println("RESPONSE(" + responsePDU.getMessage() + ") received.");
            if (responsePDU.getMessage().equals("ER")) {
                System.out.println("Exiting because of server error.");
                System.exit(-1);
            }

            byte[] buffer = new byte[4096];
            short packetNumber = 1;
            int bytesRead;
            while ((bytesRead = payloadStream.read(buffer)) != -1) {
                PDU.Data dataPDU = new PDU.Data(Arrays.copyOf(buffer, bytesRead));
                byte[] dataPDUBytes = pduSerializer.serializeData(dataPDU);
                System.out.print("Sending DATA(Packet number=" + packetNumber++ + ")...");
                out.write(dataPDUBytes);
                System.out.println("Complete");

                responsePDU = pduDeserializer.deserializeResponse(in);
                System.out.println("RESPONSE(" + responsePDU.getMessage() + ") received.");
            }

            System.out.println("Transfer complete.");
        }
        scanner.close();
    }
}
