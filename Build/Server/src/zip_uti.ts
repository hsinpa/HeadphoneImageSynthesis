import {writeFile, writeFileSync, readdir, read, emptyDirSync, mkdir} from 'fs-extra';

import AdmZip = require('adm-zip');
import {FilePath, RequestParameter} from './zip_static';
import {execFileSync, execFile} from 'child_process';

const outputFolderArray = ["C_0", "L_30", "R_30", "L_45", "R_45"];

export const ProcessRequestZipBuffer = async function(admzip:  AdmZip) {
    await emptyDirSync(FilePath.TargetInputPath);
    await emptyDirSync(FilePath.TargetOutputPath);

    // for (let folderIndex in outputFolderArray) {
    //     mkdir(FilePath.TargetOutputPath +"//"+outputFolderArray[folderIndex]);
    // }

    admzip.extractAllTo(/*target path*/ FilePath.TargetInputPath, /*overwrite*/ true);

    await WaitForMS(100);
}

export const ProcessUnityExeTask = async function(unity_exe_path : string) {
    // let childProcess = execFile(unity_exe_path);

    // await WaitForMS(2000);

    // childProcess.kill()
    
    await execFileSync(unity_exe_path);
    
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