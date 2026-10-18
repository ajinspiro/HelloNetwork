import { createConnection } from 'net'
import { readFile } from 'fs/promises'

console.log("CLIENT")

let socket = createConnection(11000, "127.0.0.1") // Socket will be created and connected.
let buffer = await readFile("IMAGE.jpg")
let isFlushedToKernalBuffer = socket.write(buffer)
if (isFlushedToKernalBuffer) {
    console.log('Sending file IMAGE.jpg complete.');
    socket.end() // Trigger graceful shutdown.
}
else {
    console.log('File is being sent. Please wait...');
    socket.on('drain', () => {
        console.log('Sending file IMAGE.jpg complete.');
        socket.end() // Trigger graceful shutdown.
    })
}