rm -r ./nupkg/ &> /dev/null

export VERSION="1.0.0-local"

# pack and install upstream-example
dotnet pack -c Release --output ./nupkg/ -p:Version=$VERSION ./samples/UpstreamExampleApp
dotnet tool uninstall -g UpstreamExampleApp
dotnet tool install UpstreamExampleApp --global --add-source ./nupkg/ --version $VERSION

# pack and install upstream-vanilla
dotnet pack -c Release --output ./nupkg/ -p:Version=$VERSION ./samples/UpstreamVanillaApp
dotnet tool uninstall -g UpstreamVanillaApp
dotnet tool install UpstreamVanillaApp --global --add-source ./nupkg/ --version $VERSION
