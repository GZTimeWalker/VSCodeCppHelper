using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace VSCodeCppHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("=================== Visual Studio Code C++ Helper V1.0.2 ===================");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("[!] 仅支持 win32-x64 操作系统并使用 TDM-GCC 作为编译器！");

            try
            {
                Console.ForegroundColor = ConsoleColor.White;

                bool vsc_install = CheckVSCode();

                bool gcc_install = CheckGCC();

                if(!vsc_install || !gcc_install)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!] 请确保VSCode和TDM-GCC已经正确安装。");
                    return;
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[*] 请选择你计划存放代码的文件夹...");

                string path = "";
                do
                {
                    Console.Write("[>] 待新建/空文件夹(默认为./Coding): ");
                    path = Console.ReadLine()!;
                    if(string.IsNullOrEmpty(path))
                        path = "Coding";
                    if(!Path.IsPathRooted(path))
                        path = Path.GetFullPath(path);
                }while(!CheckPath(path));



                Console.WriteLine($"[*] 你选择的文件夹是: \t{path}");

                (string gccpath, string gccversion) = GetGCCInfo();

                ConfigVSCode(path, gccpath, gccversion);

                TestCompile(path);

                Console.WriteLine($"[*] 在VSCode打开文件夹: {path}");

                RunCMD($"code -n {path} {Path.Combine(path,"Code/helloworld.cpp")}");

            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] 程序运行遇到错误: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(e.Message);
            }
            finally
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("===========================================================================");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("按下任意键退出...");
                Console.ReadLine();
            }
        }

        static bool CheckPath(string path)
        {
            if(!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var files = Directory.GetFiles(path);
            var dirs = Directory.GetDirectories(path);
            if(files.Length > 0 || dirs.Length > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] 使用正确字符或选择空文件夹！");
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
            return true;
        }

        static bool CheckVSCode()
        {
            string res = "";
            int code = 0;

            using (Process p = new())
            {
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.Arguments = "/c code --version";
                p.Start();
                res = p.StandardOutput.ReadToEnd();
                code = p.ExitCode;
            }

            if(code != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] 当前VSCode没有正确安装!");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[!] 从这里下载它: https://code.visualstudio.com/sha/download?build=stable&os=win32-x64-user");

                Console.ForegroundColor = ConsoleColor.Cyan;

                var help = new string[]{
                    "[-]",
                    "[-]   <===安装教程与注意事项===>",
                    "[-] 1. 将其安装在默认位置,尽量不改变安装文件夹",
                    "[-] 2. 选择附加任务时,将\"其他\"中的四项全部勾选",
                    "[-] 3. 其余依照指导依次安装",
                    "[-]",
                };

                foreach(var h in help)
                    Console.WriteLine(h);

                return false;
            }

            var data = res.Split('\n');
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[*] 当前VSCode版本为: \t{(data.Length > 0 ? data[0] : "Unknown")}");
            return true;
        }

        static bool CheckGCC()
        {
            string res = "";
            int code = 0;

            using (Process p = new())
            {
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.Arguments = "/c gcc -v";
                p.Start();
                res = p.StandardError.ReadToEnd();
                code = p.ExitCode;
            }

            if (code != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] 当前TDM-GCC没有正确安装!");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[!] 从这里下载它: https://github.com/jmeubank/tdm-gcc/releases/download/v10.3.0-tdm64-2/tdm64-gcc-10.3.0-2.exe");


                Console.ForegroundColor = ConsoleColor.Cyan;

                var help = new string[]{
                    "[-]",
                    "[-]   <===安装教程与注意事项===>",
                    "[-] 1. 将下方\"check for updated...\"取消勾选,并选择\"Create\"",
                    "[-] 2. 安装路径中不要存在中文",
                    "[-] 3. 其余依照指导依次安装",
                    "[-]",
                };

                foreach(var h in help)
                    Console.WriteLine(h);

                return false;
            }

            var data = res.Split("\r\n");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[*] 当前GCC版本为: \t{(data.Length > 3 ? data[data.Length - 2] : "Unknown")}");
            return true;
        }

        static string RunCMD(string cmd)
        {
            string res = "";
            int code = 0;

            using (Process p = new())
            {
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.Arguments = "/c " + cmd;
                p.Start();
                res = p.StandardOutput.ReadToEnd();
                code = p.ExitCode;
            }

            if(code != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[!] 运行失败: {cmd}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            return res;
        }

        static (string, string) GetGCCInfo()
        {
            string res = "";
            int code = 0;

            using (Process p = new())
            {
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.Arguments = "/c gcc -v";
                p.Start();
                res = p.StandardError.ReadToEnd();
                code = p.ExitCode;
            }

            var data = res.Split("\r\n")[2];
            string path = data[20..(data.IndexOf("/../libexec") - 4)];
            Console.WriteLine($"[*] 当前GCC路径为: \t{path}");
            string version = data[(data.IndexOf("mingw32/") + 8)..(data.IndexOf("/lto"))];
            return (path, version);
        }

        static void ConfigVSCode(string path, string gccpath, string gccversion)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                path = Path.Combine(path, ".vscode");
                Directory.CreateDirectory(path);

                string launch = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("VSCodeCppHelper.Assets.launch.json")!).ReadToEnd();
                string tasks = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("VSCodeCppHelper.Assets.tasks.json")!).ReadToEnd();
                string cpp = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("VSCodeCppHelper.Assets.c_cpp_properties.json")!).ReadToEnd();

                launch = launch.Replace("{GCCPATH}", gccpath);
                cpp = cpp.Replace("{GCCPATH}", gccpath)
                    .Replace("{GCCVERSION}", gccversion);

                Console.WriteLine($"[>] 写入{Path.Combine(path, "launch.json")}");
                File.WriteAllText(Path.Combine(path, "launch.json"), launch);
                Console.WriteLine($"[>] 写入{Path.Combine(path, "tasks.json")}");
                File.WriteAllText(Path.Combine(path, "tasks.json"), tasks);
                Console.WriteLine($"[>] 写入{Path.Combine(path, "c_cpp_properties.json")}");
                File.WriteAllText(Path.Combine(path, "c_cpp_properties.json"), cpp);

                var extlist = RunCMD("code --list-extensions");
                var exts = new string[]{ "ms-vscode.cpptools", "austin.code-gnu-global", "formulahendry.code-runner" };

                foreach(string ext in exts)
                {
                    if (!extlist.Contains(ext))
                    {
                        Console.WriteLine($"[*] 安装VSCode扩展: {ext}");
                        RunCMD($"code --install-extension {ext}");
                    }
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch
            {
                throw;
            }
        }

        static void TestCompile(string path)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Directory.CreateDirectory(Path.Combine(path, "Debug"));

                string codepath = Path.Combine(path, "Code");
                Directory.CreateDirectory(codepath);

                string hello = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("VSCodeCppHelper.Assets.helloworld.cpp")!).ReadToEnd();

                Console.WriteLine($"[>] 写入{Path.Combine(codepath, "helloworld.cpp")}");
                File.WriteAllText(Path.Combine(codepath, "helloworld.cpp"), hello);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[*] 测试编译hello world...");
                string output = RunCMD($"g++ -g {Path.Combine(codepath, "helloworld.cpp")} -o {Path.Combine(path, "Debug/helloworld.exe")}");

                Console.WriteLine($"[*] 测试运行hello world...");
                output = RunCMD(Path.Combine(path, "Debug/helloworld.exe"));
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"[+] {output.Trim()}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch
            {
                throw;
            }
        }
    }
}
