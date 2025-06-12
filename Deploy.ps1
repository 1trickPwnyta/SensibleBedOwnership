$path = "$env:RIMWORLD\Mods\$(($PSScriptRoot | gi).Name)"

Copy-Item "$path\About\PublishedFileId.txt" "$PSScriptRoot\About"
Remove-Item -Recurse "$path\*"
mkdir $path
@(
	".",
	"1.2",
	"1.3",
	"1.4",
	"1.5",
	"1.6"
) | %{
	$base = $_
	@(
		"About",
		"Assemblies",
		"Defs",
		"Languages",
		"Patches",
		"Songs",
		"Sounds",
		"Source",
		"Textures"
	) | %{ Copy-Item -Recurse "$PSScriptRoot\$base\$_" "$path\$base\$_" }
	Remove-Item -Recurse "$path\$base\Source\bin"
	Remove-Item -Recurse "$path\$base\Source\obj"
	Remove-Item "$path\$base\Source\packages.config"
}
