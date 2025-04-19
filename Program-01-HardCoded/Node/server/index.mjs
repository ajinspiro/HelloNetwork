import { createServer } from 'net'
import { writeFile } from 'fs/promises'

console.log("SERVER")

let buffers = []

const server = createServer((socket) => {
    socket.on('data', (data) => {
        buffers.push(data)
    });

    socket.on('end', async () => {
        let imageBytesFull = Buffer.concat(buffers)
        await writeFile("IMAGE.jpg", imageBytesFull)
        buffers = []
        console.log("File received.")
    });

    socket.on('error', (err) => {
        buffers = []
        console.error('Socket error:', err);
    });
});

server.listen(11000, '127.0.0.1', () => {
    console.log('TCP server listening on port 11000');
});
