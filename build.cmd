mkdir bin
ilasm /dll src\PtrUtils.il /out:bin\System.Slice.netmodule
csc /t:library /debug:pdbonly /unsafe /o+ /addmodule:bin\System.Slice.netmodule /out:bin\System.Slice.dll src\*.cs
csc /debug:pdbonly /unsafe /o+ /r:bin\System.Slice.dll /out:bin\System.Slice.Test.exe tests\*.cs

