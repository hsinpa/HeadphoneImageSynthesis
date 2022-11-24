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
Object.defineProperty(exports, "__esModule", { value: true });
exports.ReadStringToFile = exports.WriteStringToFile = exports.WaitUntil = exports.ReadFolder = exports.WaitForMS = exports.ProcessUnityExeTask = exports.ProcessRequestZipBuffer = void 0;
const fs_extra_1 = require("fs-extra");
const AdmZip = require("adm-zip");
const zip_static_1 = require("./zip_static");
const child_process_1 = require("child_process");
const outputFolderArray = ["C_0", "L_30", "R_30", "L_45", "R_45"];
let m_childProcess;
const ProcessRequestZipBuffer = function (admzip) {
    return __awaiter(this, void 0, void 0, function* () {
        yield (0, fs_extra_1.emptyDirSync)(zip_static_1.FilePath.TargetInputPath);
        yield (0, fs_extra_1.emptyDirSync)(zip_static_1.FilePath.TargetOutputPath);
        admzip.extractAllTo(zip_static_1.FilePath.TargetInputPath, true);
        (0, exports.WriteStringToFile)(zip_static_1.FilePath.FlagFilePath, "0");
        yield (0, exports.WaitForMS)(100);
    });
};
exports.ProcessRequestZipBuffer = ProcessRequestZipBuffer;
const ProcessUnityExeTask = function (unity_exe_path) {
    return __awaiter(this, void 0, void 0, function* () {
        if (m_childProcess == null || m_childProcess.exitCode != null)
            m_childProcess = (0, child_process_1.execFile)(unity_exe_path);
        yield (0, exports.WaitUntil)(() => {
            let flagResult = (0, exports.ReadStringToFile)(zip_static_1.FilePath.FlagFilePath);
            return flagResult == "1";
        });
        let files = yield (0, exports.ReadFolder)(zip_static_1.FilePath.TargetOutputPath);
        let zip = new AdmZip();
        zip.addLocalFolder(zip_static_1.FilePath.TargetOutputPath);
        return yield zip.toBufferPromise();
    });
};
exports.ProcessUnityExeTask = ProcessUnityExeTask;
const WaitForMS = function (ms) {
    return __awaiter(this, void 0, void 0, function* () {
        return new Promise(resolve => setTimeout(resolve, ms));
    });
};
exports.WaitForMS = WaitForMS;
const ReadFolder = function (path) {
    return __awaiter(this, void 0, void 0, function* () {
        return new Promise(resolve => (0, fs_extra_1.readdir)(zip_static_1.FilePath.TargetOutputPath, (err, files) => {
            resolve(files);
        }));
    });
};
exports.ReadFolder = ReadFolder;
const WaitUntil = function (condition) {
    return __awaiter(this, void 0, void 0, function* () {
        return yield new Promise(resolve => {
            const interval = setInterval(() => {
                if (condition()) {
                    resolve('');
                    clearInterval(interval);
                }
                ;
            }, 1000);
        });
    });
};
exports.WaitUntil = WaitUntil;
const WriteStringToFile = function (path, content) {
    try {
        (0, fs_extra_1.writeFile)(path, content);
    }
    catch (err) {
        console.error(err);
    }
};
exports.WriteStringToFile = WriteStringToFile;
const ReadStringToFile = function (path) {
    return (0, fs_extra_1.readFileSync)(path, { encoding: 'utf8' });
};
exports.ReadStringToFile = ReadStringToFile;
//# sourceMappingURL=zip_uti.js.map