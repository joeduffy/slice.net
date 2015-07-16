mkdir bin
ilasm /dll /debug src\PtrUtils.il /out:bin\System.Slice.Core.dll
csc /t:library /debug /unsafe /o+ /r:bin\System.Slice.Core.dll /out:bin\System.Slice.dll src\*.cs
csc /unsafe /r:bin\System.Slice.dll /out:bin\System.Slice.Test.exe tests\*.cs