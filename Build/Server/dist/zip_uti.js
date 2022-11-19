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
exports.ReadFolder = exports.WaitForMS = exports.ProcessUnityExeTask = exports.ProcessRequestZipBuffer = void 0;
const fs_extra_1 = require("fs-extra");
const AdmZip = require("adm-zip");
const zip_static_1 = require("./zip_static");
const child_process_1 = require("child_process");
const outputFolderArray = ["C_0", "L_30", "R_30", "L_45", "R_45"];
const ProcessRequestZipBuffer = function (admzip) {
    return __awaiter(this, void 0, void 0, function* () {
        yield (0, fs_extra_1.emptyDirSync)(zip_static_1.FilePath.TargetInputPath);
        yield (0, fs_extra_1.emptyDirSync)(zip_static_1.FilePath.TargetOutputPath);
        admzip.extractAllTo(zip_static_1.FilePath.TargetInputPath, true);
        yield (0, exports.WaitForMS)(100);
    });
};
exports.ProcessRequestZipBuffer = ProcessRequestZipBuffer;
const ProcessUnityExeTask = function (unity_exe_path) {
    return __awaiter(this, void 0, void 0, function* () {
        yield (0, child_process_1.execFileSync)(unity_exe_path);
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
//# sourceMappingURL=zip_uti.js.map