mkdir bin

mkdir bin\Debug
ilasm /dll src\PtrUtils.il /out:bin\Debug\System.Slice.netmodule
csc /t:library /debug /unsafe /o+ /addmodule:bin\System.Slice.netmodule /out:bin\Debug\System.Slice.dll src\*.cs
csc /debug /unsafe /r:bin\Debug\System.Slice.dll /out:bin\Debug\System.Slice.Test.exe tests\*.cs

mkdir bin\Release
ilasm /dll src\PtrUtils.il /out:bin\Release\System.Slice.netmodule
csc /t:library /unsafe /o+ /addmodule:bin\System.Slice.netmodule /out:bin\Release\System.Slice.dll src\*.cs
csc /unsafe /r:bin\Release\System.Slice.dll /out:bin\Release\System.Slice.Test.exe tests\*.cs