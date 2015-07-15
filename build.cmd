mkdir bin
ilasm /dll /debug src\PtrUtils.il /out:bin\System.Slice.Core.dll
csc /t:library /debug /unsafe src\*.cs /r:bin\System.Slice.Core.dll /out:bin\System.Slice.dll
csc /unsafe tests\*.cs /r:bin\System.Slice.dll /out:bin\System.Slice.Test.exe

