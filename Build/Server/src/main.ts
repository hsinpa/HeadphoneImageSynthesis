import Fastify, { FastifyInstance, RouteShorthandOptions } from 'fastify'
import { Server, IncomingMessage, ServerResponse } from 'http'
import Multer from 'fastify-multer';
import fastify_static from '@fastify/static';
import {writeFile, writeFileSync, createReadStream} from 'fs';
import AdmZip = require('adm-zip');
import {FilePath, RequestParameter} from './zip_static';
import {ProcessRequestZipBuffer, ProcessUnityExeTask} from './zip_uti';
import {PassThrough} from 'stream';

import {join} from 'path';
const server: FastifyInstance = Fastify({})
const storage = Multer.memoryStorage();
const multer = Multer({ storage: storage });
let process_zip_flag : boolean = false;

server.register(multer.contentParser);
server.get('/ping', async (request, reply) => {
  return { pong: 'it worked!' }
});

server.post('/api/try_on', { preHandler: multer.single('model_images') }, async (request :any, reply) => {
  if (process_zip_flag) {
    reply.code(200).send('BUSY');
    return;
  }

  process_zip_flag = true;

  try {
    await writeFileSync(FilePath.HttpOutputPath, request.file.buffer);

    var zip_reader = new AdmZip(FilePath.HttpOutputPath);
  
    await ProcessRequestZipBuffer(zip_reader);
  
    let buffer : Buffer = await ProcessUnityExeTask(FilePath.UnityEXEPath);
  
    const readStream = new PassThrough();
    readStream.end(buffer);
  
    process_zip_flag = false;
  
    reply.raw.writeHead(200, { 'Content-Type': 'application/zip, application/octet-stream', 'Content-disposition' :  'attachment; filename=output.zip'});
  
    readStream.pipe(reply.raw);

  } catch(e) {

    process_zip_flag = false;
    reply.code(200).send('Fail');

  }
});

const start = async () => {
  try {
    await server.listen({ port: 3010 })

    const address = server.server.address()
    const port = typeof address === 'string' ? address : address?.port

  } catch (err) {
    server.log.error(err)
    process.exit(1)
  }
}
start()