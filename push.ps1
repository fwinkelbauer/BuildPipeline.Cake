$packages = Get-ChildItem *.nupkg

foreach ($package in $packages)
{
  nuget push $package -source https://www.nuget.org/api/v2/package
  Remove-Item $package
}
