{
  description = "Sessia";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
  };

  outputs = { self, nixpkgs }:
    let
      system = "x86_64-linux";
      pkgs = import nixpkgs { inherit system; };
    in {
      devShells.${system}.default = pkgs.mkShell {
        packages = [
          pkgs.dotnet-sdk_9
          pkgs.dotnet-runtime_9
        ];

        DOTNET_ROOT = pkgs.dotnet-sdk_9;
        DOTNET_CLI_TELEMETRY_OPTOUT = "1";
        DOTNET_NOLOGO = "1";

        shellHook = ''
          export NUGET_PACKAGES="$PWD/.nuget-packages"

          export DOTNET_SYSTEM_NET_DISABLEIPV6=1
          export DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
          export DOTNET_SYSTEM_NET_HTTP_SOCKETSHTTPHANDLER_HTTP2SUPPORT=0
          export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

          # Установка dotnet-ef локально в проект
          if [ ! -f "$PWD/.config/dotnet-tools.json" ]; then
            echo "Installing dotnet-ef as local tool..."
            dotnet new tool-manifest 2>/dev/null || true
            dotnet tool install dotnet-ef
          fi

          export PATH="$PWD/.config/dotnet-tools:$PATH"

          echo "dotnet: $(dotnet --version)"
          echo "NuGet packages: $NUGET_PACKAGES"
          echo "Run 'dotnet ef database update' to apply migrations"
        '';
      };
    };
}
