"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const fastify_1 = __importDefault(require("fastify"));
const fastify_multer_1 = __importDefault(require("fastify-multer"));
const fs_1 = require("fs");
const AdmZip = require("adm-zip");
const zip_static_1 = require("./zip_static");
const zip_uti_1 = require("./zip_uti");
const stream_1 = require("stream");
const server = (0, fastify_1.default)({});
const storage = fastify_multer_1.default.memoryStorage();
const multer = (0, fastify_multer_1.default)({ storage: storage });
let process_zip_flag = false;
server.register(multer.contentParser);
server.get('/ping', (request, reply) => __awaiter(void 0, void 0, void 0, function* () {
    return { pong: 'it worked!' };
}));
server.post('/api/try_on', { preHandler: multer.single('model_images') }, (request, reply) => __awaiter(void 0, void 0, void 0, function* () {
    if (process_zip_flag) {
        reply.code(200).send('BUSY');
        return;
    }
    process_zip_flag = true;
    try {
        yield (0, fs_1.writeFileSync)(zip_static_1.FilePath.HttpOutputPath, request.file.buffer);
        var zip_reader = new AdmZip(zip_static_1.FilePath.HttpOutputPath);
        yield (0, zip_uti_1.ProcessRequestZipBuffer)(zip_reader);
        let buffer = yield (0, zip_uti_1.ProcessUnityExeTask)(zip_static_1.FilePath.UnityEXEPath);
        const readStream = new stream_1.PassThrough();
        readStream.end(buffer);
        process_zip_flag = false;
        reply.raw.writeHead(200, { 'Content-Type': 'application/zip, application/octet-stream', 'Content-disposition': 'attachment; filename=output.zip' });
        readStream.pipe(reply.raw);
    }
    catch (e) {
        process_zip_flag = false;
        reply.code(200).send('Fail');
    }
}));
const start = () => __awaiter(void 0, void 0, void 0, function* () {
    try {
        yield server.listen({ port: 3010 });
        const address = server.server.address();
        const port = typeof address === 'string' ? address : address === null || address === void 0 ? void 0 : address.port;
    }
    catch (err) {
        server.log.error(err);
        process.exit(1);
    }
});
start();
//# sourceMappingURL=main.js.map