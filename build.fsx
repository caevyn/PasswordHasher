// include Fake lib
#r @"tools\FAKE\tools\FakeLib.dll"
open Fake
open Fake.AssemblyInfoFile
 
RestorePackages()

 // Properties
let buildDir = "./build/"
let testDir  = "./test/"
let nugetDir = "./nuget/output/"
let packagingDir = "./nuget/working/"
let version = environVarOrDefault "VERSION" "1.0.0.0"
let projectDescription = "IPasswordHasher implementation for Asp.Net Identity"
let projectName = "Malt.PasswordHasher"

Target "Clean" (fun _ ->
    CleanDir buildDir
    CleanDir "./nuget"
    CleanDir "./test"
)

Target "AssemblyInfo" (fun _ ->
    CreateCSharpAssemblyInfo "./src/Malt.PasswordHasher/Properties/AssemblyInfo.cs"
        [Attribute.Title "Malt.PasswordHasher"
         Attribute.Description projectDescription
         Attribute.Product "PasswordHasher"
         Attribute.Version version
         Attribute.FileVersion version]
)
       
Target "BuildApp" (fun _ ->
    !! "src/Malt.PasswordHasher/*.csproj"
      |> MSBuildRelease buildDir "Build"
      |> Log "AppBuild-Output: "
)
 
Target "BuildTest" (fun _ ->
    !! "src/Malt.PasswordHasher.Test/*.csproj"
      |> MSBuildDebug testDir "Build"
      |> Log "TestBuild-Output: "
)

Target "Test" (fun _ ->
    !! (testDir + "/*.Test.dll") 
      |> NUnit (fun p ->
          {p with
             DisableShadowCopy = true;
             OutputFile = testDir + "TestResults.xml" })
)

Target "CreatePackage" (fun _ ->
    //Create required dirs and Copy all the package files into a package folder
    CreateDir nugetDir
    CreateDir packagingDir
    CreateDir (packagingDir @@ "/lib")
    CreateDir (packagingDir @@ "/lib/net451")

    CopyFiles (packagingDir @@ "/lib/net451") [buildDir @@ "/Malt.PasswordHasher.dll" ; buildDir @@ "/Malt.PasswordHasher.pdb"]

    NuGet (fun p -> 
        {p with
            Authors = ["Matt Murphy"]
            Project = projectName
            Description = projectDescription                               
            OutputPath = nugetDir
            Summary = "Asp.Net Identity PBKDF2 IPasswordHasher"
            WorkingDir = packagingDir
            Version = version
            Dependencies =
             ["CryptSharp",  GetPackageVersion "./packages/" "CryptSharp"
              "Microsoft.AspNet.Identity.Core", GetPackageVersion "./packages/" "Microsoft.AspNet.Identity.Core"]
            AccessKey = ""
            Publish = false }) 
            "Malt.PasswordHasher.nuspec"
)

// Default target
Target "Default" (fun _ ->
    trace "Running Default target..."
)

// Dependencies
"Clean"
  ==> "AssemblyInfo"
  ==> "BuildApp"
  ==> "BuildTest"
  ==> "Test"
  ==> "CreatePackage"
  ==> "Default"

// start build
RunTargetOrDefault "Default"