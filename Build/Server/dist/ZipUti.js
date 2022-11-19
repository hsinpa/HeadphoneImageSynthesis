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
exports.ProcessRequestZipBuffer = void 0;
const zip_static_1 = require("./zip_static");
const ProcessRequestZipBuffer = function (admzip) {
    return __awaiter(this, void 0, void 0, function* () {
        admzip.extractAllTo(zip_static_1.FilePath.TargetInputPath, true);
    });
};
exports.ProcessRequestZipBuffer = ProcessRequestZipBuffer;
//# sourceMappingURL=ZipUti.js.map