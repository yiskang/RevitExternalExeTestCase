#include <Windows.h>
#include <iostream>
#include <fstream>

std::string ExePath() {
    TCHAR buffer[MAX_PATH] = { 0 };
    GetModuleFileName(NULL, buffer, MAX_PATH);
    std::wstring::size_type pos = std::wstring(buffer).find_last_of(L"\\/");
    std::wstring path = std::wstring(buffer).substr(0, pos);

    std::string result(path.begin(), path.end());
    return result;
}

int main(int argc, char* argv[])
{
    std::string appPath = argv[0];
    std::string appName = appPath.substr(appPath.find_last_of("/\\") + 1);
    std::string filename = "test.txt";//argv[1];

    std::cout << appName << ": is under `" << appPath.substr(0, appPath.find_last_of("/\\")) << "`..." << std::endl;

    try {
        std::cout << appName << ": Current Working Directory is `" << ExePath() << "`..." << std::endl;

        std::ofstream outfile;
        outfile.exceptions(std::ifstream::failbit | std::ifstream::badbit);

        try {    
            std::cout << appName << ": Creating & Opening the file `" << filename << "`..."  << std::endl;
            outfile.open(filename);
            std::cout << appName << ": - DONE" << std::endl;

            std::cout << appName << ": Ouputing to the file ... `" << filename << "`..." << std::endl;
            outfile << "test" << std::endl;

            outfile.flush();
            std::cout << appName << ": - DONE" << std::endl;

            std::cout << appName << ": Saving & Closing the file `" << filename << "`..." << std::endl;
            outfile.close();
            std::cout << appName << ": - DONE" << std::endl;
        }
        catch (std::ifstream::failure e) {
            std::cerr << appName << ": Exception opening/writing/closing the file `" << filename << std::endl;
        }
    }
    catch (std::exception e) {
        std::cerr << appName << ": Exception starting the process" << std::endl;
    }

    return 0;
}