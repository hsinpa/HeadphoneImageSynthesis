import {writeFile, writeFileSync, readdir, read, emptyDirSync, mkdir, readFileSync} from 'fs-extra';

import AdmZip = require('adm-zip');
import {FilePath, RequestParameter} from './zip_static';
import {execFileSync, execFile, ChildProcess} from 'child_process';

const outputFolderArray = ["C_0", "L_30", "R_30", "L_45", "R_45"];

let m_childProcess: ChildProcess;

export const ProcessRequestZipBuffer = async function(admzip:  AdmZip) {
    await emptyDirSync(FilePath.TargetInputPath);
    await emptyDirSync(FilePath.TargetOutputPath);

    // for (let folderIndex in outputFolderArray) {
    //     mkdir(FilePath.TargetOutputPath +"//"+outputFolderArray[folderIndex]);
    // }

    admzip.extractAllTo(/*target path*/ FilePath.TargetInputPath, /*overwrite*/ true);

    WriteStringToFile(FilePath.FlagFilePath, "0");

    await WaitForMS(100);
}

export const ProcessUnityExeTask = async function(unity_exe_path : string) {
    if (m_childProcess == null || m_childProcess.exitCode != null)
        m_childProcess = execFile(unity_exe_path);
    
    //await execFileSync(unity_exe_path);
    
    await WaitUntil( () : boolean => {
        let flagResult = ReadStringToFile(FilePath.FlagFilePath);
        return flagResult == "1";
    } );

    // await execFileSync(unity_exe_path);
    
    let files = await ReadFolder(FilePath.TargetOutputPath); 
    let zip = new AdmZip();
    zip.addLocalFolder(FilePath.TargetOutputPath);
    // files.forEach(x => {
        
    //     console.log(x);

    //     // let full_path = FilePath.TargetOutputPath +"//" + x;
    //     // zip.addLocalFile(full_path);
    // });

    return await zip.toBufferPromise();
}

export const WaitForMS = async function(ms: number) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

export const ReadFolder = async function(path: string) : Promise<string[]>{
    return new Promise(resolve => readdir(FilePath.TargetOutputPath, (err, files) => {
        resolve(files);
    }));
}

export const WaitUntil = async function(condition: () => boolean) {
    return await new Promise(resolve => {
        const interval = setInterval(() => {
        if (condition()) {
            resolve('');
            clearInterval(interval);
        };
        }, 1000);
    });
}

export const WriteStringToFile = function(path: string, content: string) {
    try {
        writeFile(path, content);
      } catch (err) {
        console.error(err);
      }
}

export const ReadStringToFile = function(path: string) {
    return readFileSync(path, { encoding: 'utf8' });
}