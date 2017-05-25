Get-ChildItem *.nupkg | Remove-Item
nuget pack Cake.Mug\Cake.Mug.nuspec
nuget pack Cake.Mug.Tools\Cake.Mug.Tools.nuspec
