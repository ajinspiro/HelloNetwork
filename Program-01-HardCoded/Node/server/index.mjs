import { createServer } from 'net'
import { writeFile, appendFile } from 'fs/promises'

console.log("SERVER")

let buffers = []

const server = createServer((socket) => {

    socket.on('data', async (data) => {
        if (buffers.map(x => x.length).reduce((acc, curr) => acc + curr, 0) < 90674) {  // Limitation: server needs to know the size of the transferred file in advance.
            buffers.push(data)
        } else {
            let imageBytesFull = Buffer.concat(buffers)
            await writeFile("IMAGE.jpg", imageBytesFull)
            buffers = []
            console.log("File received.")
        }
    });

    socket.on('error', (err) => {
        console.error('Socket error:', err);
    });
});

server.listen(11000, '127.0.0.1', () => {
    console.log('TCP server listening on port 11000');
});
