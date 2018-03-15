# hikvision-console-capture

海康威视命令行工具，拍照生成jpeg文件

USAGE:

Simple:

hikvision capture -i 192.168.3.64

Full:

hikvision capture -i 192.168.3.64 -o c:\capture\capture.jpg -p haikang123 --port 8000 -u admin

  -i           Required. (Default: 192.168.3.64) The IP of carema.

  --port       (Default: 8000) The port of carema.

  -o           (Default: capture.jpg) The output jpeg file path.

  -u           (Default: admin) The user name for login carema.

  -p           (Default: haikang123) The password for login carema..

  --help       Display this help screen.

  --version    Display version information.
