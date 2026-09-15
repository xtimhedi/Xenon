using System;
using System.Collections.Generic;
using System.Text;

namespace Editor.ProjectSystem
{
    public static class Templates
    {
        public static ProjectFileTemplate ProjFileTemplate = new ProjectFileTemplate(
            Globals.CurrentProjectCtx,
            @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <EmbeddedResource Include=""Resources\**"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\Xenon\Xenon.csproj"" />
  </ItemGroup>

	<ItemGroup>
		<TrimmerRootAssembly Include=""Veldrid"" />
		<TrimmerRootAssembly Include=""Veldrid.StartupUtilities"" />
		<TrimmerRootAssembly Include=""Vortice.DXGI"" />
		<TrimmerRootAssembly Include=""Vortice.Mathematics"" />
		<TrimmerRootAssembly Include=""SharpGen.Runtime"" />
		<TrimmerRootAssembly Include=""Xenon"" />
		<TrimmerRootAssembly Include=""{Globals.CurrentProjectCtx.ProjectNamespace}"" />
	</ItemGroup>

</Project>
");
    }
}
