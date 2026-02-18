import com.jetbrains.plugin.structure.base.utils.isFile
import org.gradle.api.tasks.testing.logging.TestExceptionFormat
import org.jetbrains.changelog.exceptions.MissingVersionException
import org.jetbrains.intellij.platform.gradle.Constants
import org.jetbrains.intellij.platform.gradle.TestFrameworkType
import org.jetbrains.kotlin.gradle.tasks.KotlinCompile
import kotlin.io.path.absolute
import kotlin.io.path.isDirectory

plugins {
    alias(libs.plugins.changelog)
    alias(libs.plugins.gradleIntelliJPlatform)
    alias(libs.plugins.gradleJvmWrapper)
    alias(libs.plugins.kotlinJvm)
    id("java")
}

allprojects {

    repositories {
        maven("https://cache-redirector.jetbrains.com/intellij-dependencies")
        mavenCentral()
    }
}

repositories {
    intellijPlatform {
        defaultRepositories()
        jetbrainsRuntime()
    }
}

val pluginVersion: String by project
val untilBuildVersion: String by project
val buildConfiguration: String by project
val dotNetPluginId: String by project

version = pluginVersion

// ============ Plugin File Definitions (Single Source of Truth) ==============

val pluginStagingDir = layout.buildDirectory.dir("plugin-staging").get().asFile
val pluginContentDir = file("${pluginStagingDir}/${rootProject.name}")
val signingManifestFile = file("${pluginStagingDir}/files-to-sign.txt")

val dotNetSrcDir = File(projectDir, "src/dotnet")
val dotNetOutputDir = "$dotNetSrcDir/$dotNetPluginId/bin/$dotNetPluginId/$buildConfiguration"

// All .NET files to include in the plugin
val dotNetOutputFiles = listOf(
    "${dotNetPluginId}.dll",
    "${dotNetPluginId}.pdb",
)

// .NET files that need signing (only our own code)
val dotNetFilesToSign = listOf(
    "${dotNetPluginId}.dll",
)

// JAR files that need signing (only our own code)
// Note: no searchable options JAR because buildSearchableOptions = false
val jarFilesToSign = listOf(
    "${rootProject.name}-${version}.jar",
)

val riderSdkPath by lazy {
    val path = intellijPlatform.platformPath.resolve("lib/DotNetSdkForRdPlugins").absolute()
    if (!path.isDirectory()) error("$path does not exist or not a directory")

    println("Rider SDK path: $path")
    return@lazy path
}

dependencies {
    intellijPlatform {
        with(file("build/rider")) {
            when {
                exists() -> {
                    logger.lifecycle("*** Using Rider SDK from local path $this")
                    local(this)
                }

                else -> {
                    logger.lifecycle("*** Using Rider SDK from intellij-snapshots repository")
                    rider(libs.versions.riderSdk, useInstaller = false)
                }
            }
        }

        jetbrainsRuntime()

        testFramework(TestFrameworkType.Bundled)
    }
}

intellijPlatform {
    this.instrumentCode = false
    this.buildSearchableOptions = false
}


kotlin {
    jvmToolchain {
        languageVersion = JavaLanguageVersion.of(17)
    }
}

sourceSets {
    main {
        kotlin.srcDir("src/rider/generated/kotlin")
        kotlin.srcDir("src/rider/main/kotlin")
        resources.srcDir("src/rider/main/resources")
    }
}


tasks {
    val generateDotNetSdkProperties by registering {
        val dotNetSdkGeneratedPropsFile = File(projectDir, "build/DotNetSdkPath.Generated.props")
        doLast {
            dotNetSdkGeneratedPropsFile.writeTextIfChanged("""<Project>
  <PropertyGroup>
    <DotNetSdkPath>$riderSdkPath</DotNetSdkPath>
  </PropertyGroup>
</Project>
""")
        }
    }

    val generateNuGetConfig by registering {
        val nuGetConfigFile = File(dotNetSrcDir, "nuget.config")
        doLast {
            nuGetConfigFile.writeTextIfChanged("""
            <?xml version="1.0" encoding="utf-8"?>
            <!-- Auto-generated from 'generateNuGetConfig' task of old.build_gradle.kts -->
            <!-- Run `gradlew :prepare` to regenerate -->
            <configuration>
                <packageSources>
                    <add key="rider-sdk" value="$riderSdkPath" />
                </packageSources>
            </configuration>
            """.trimIndent())
        }
    }

    val rdGen = ":protocol:rdgen"

    register("prepare") {
        dependsOn(rdGen, generateDotNetSdkProperties, generateNuGetConfig)
    }

    val compileDotNet by registering {
        dependsOn(rdGen, generateDotNetSdkProperties, generateNuGetConfig)
        doLast {
            exec {
                executable(layout.projectDirectory.file("dotnet.cmd"))
                args("build", "-consoleLoggerParameters:ErrorsOnly", "--configuration", buildConfiguration)
            }
        }
    }

    withType<KotlinCompile> {
        dependsOn(rdGen)
    }

    patchPluginXml {
        untilBuild.set(untilBuildVersion)
        val latestChangelog = try {
            changelog.getUnreleased()
        } catch (_: MissingVersionException) {
            changelog.getLatest()
        }
        changeNotes.set(provider {
            changelog.renderItem(
                latestChangelog
                    .withHeader(false)
                    .withEmptySections(false),
                org.jetbrains.changelog.Changelog.OutputType.HTML
            )
        })
    }

    prepareSandbox {
        dependsOn(compileDotNet)

        // Use shared .NET output files list
        dotNetOutputFiles.forEach { fileName ->
            from("${dotNetOutputDir}/${fileName}") {
                into("${rootProject.name}/dotnet")
            }
        }

        doLast {
            // Validation: ensure all .NET output files exist
            dotNetOutputFiles.forEach { fileName ->
                val file = file("${dotNetOutputDir}/${fileName}")
                if (!file.exists()) throw RuntimeException("File ${file} does not exist")
            }
        }
    }

    prepareTestSandbox {
        dependsOn(compileDotNet)

        dotNetOutputFiles.forEach { fileName ->
            from("${dotNetOutputDir}/${fileName}") {
                into("${rootProject.name}/dotnet")
            }
        }
    }

    withType<Test> {
        classpath -= classpath.filter {
            (it.name.startsWith("localization-") && it.name.endsWith(".jar")) // TODO: https://youtrack.jetbrains.com/issue/IJPL-178084/External-plugin-tests-break-due-to-localization-issues
                || it.name == "cwm-plugin.jar" // TODO: Check after 251 EAP5 release
        }

        useTestNG()
        testLogging {
            showStandardStreams = true
            exceptionFormat = TestExceptionFormat.FULL
        }
        environment["LOCAL_ENV_RUN"] = "true"
    }

    val testRiderPreview by intellijPlatformTesting.testIde.registering {
        version = libs.versions.riderSdkPreview
        useInstaller = false
        task {
            enabled = libs.versions.riderSdk.get() != libs.versions.riderSdkPreview.get()
        }
    }

    check { dependsOn(testRiderPreview.name) }

    runIde {
        jvmArgs("-Xmx1500m")
    }
}

// ========= Two-Phase Build for Signing Support ================

// Preparation for signing. Build all dll's and jar's and puts them into ${pluginStagingDir}
val preparePluginForSigning by tasks.registering(Sync::class) {
    description = "Prepares plugin files for signing and generates signing manifest"
    group = "build"

    // Source 1: Copy the full plugin directory from prepareSandbox
    from(tasks.prepareSandbox.map { it.pluginDirectory })

    // Source 2: Copy searchable options JAR to lib/ (when enabled)
    if (intellijPlatform.buildSearchableOptions.get()) {
        from(tasks.jarSearchableOptions.map { it.archiveFile }) {
            into("lib")
        }
    }

    // Destination: the plugin content directory inside staging
    into(pluginContentDir)

    // After syncing, generate the signing manifest
    doLast {
        val filesToSign = mutableListOf<String>()

        // Add JAR files that need signing (only our own code)
        jarFilesToSign.forEach { jarName ->
            filesToSign.add("${rootProject.name}/lib/${jarName}")
        }

        // Add .NET files that need signing
        dotNetFilesToSign.forEach { fileName ->
            filesToSign.add("${rootProject.name}/dotnet/${fileName}")
        }

        // Write manifest
        signingManifestFile.writeText(filesToSign.joinToString("\n"))

        // Summary
        println("Plugin prepared for signing: ${pluginStagingDir}")
        println("Signing manifest: ${signingManifestFile}")
        println("Files to sign: ${filesToSign.size}")
        filesToSign.forEach { println("  - $it") }
    }
}

// Validates that ${pluginStagingDir} has all required files to assemble the plugin
val validatePluginStaging by tasks.registering {
    description = "Validates that plugin staging directory exists and contains required files"
    group = "build"

    doLast {
        if (!pluginContentDir.exists()) {
            throw RuntimeException(
                "Plugin staging directory not found: ${pluginContentDir}\n" +
                "Run './gradlew preparePluginForSigning' first."
            )
        }

        // Validate expected .NET output files exist
        dotNetOutputFiles.forEach { fileName ->
            val file = file("${pluginContentDir}/dotnet/${fileName}")
            if (!file.exists()) throw RuntimeException("Expected .NET file not found: ${file}")
        }

        // Validate expected JAR files exist
        jarFilesToSign.forEach { jarName ->
            val file = file("${pluginContentDir}/lib/${jarName}")
            if (!file.exists()) throw RuntimeException("Expected JAR file not found: ${file}")
        }
    }
}

// Assembles the final zip-archive by taking the files from ${pluginStagingDir}
val assemblePlugin by tasks.registering(Zip::class) {
    description = "Assembles the final plugin ZIP from staged files"
    group = "build"

    dependsOn(validatePluginStaging)

    from(pluginStagingDir) {
        // Include plugin content directory
        include("${rootProject.name}/**")
        // Exclude signing manifest from final ZIP
        exclude("files-to-sign.txt")
    }

    // Use same naming convention as original BuildPluginTask:
    // archiveBaseName comes from plugin.xml (via IntelliJ plugin extension)
    archiveBaseName.convention(intellijPlatform.projectName)
    destinationDirectory.set(layout.buildDirectory.dir("distributions"))

    // Register artifact to INTELLIJ_PLATFORM_DISTRIBUTION configuration (same as original BuildPluginTask)
    // This ensures publishPlugin and other dependent tasks can find the archive
    val intellijPlatformDistributionConfiguration =
        configurations[Constants.Configurations.INTELLIJ_PLATFORM_DISTRIBUTION]
    artifacts.add(intellijPlatformDistributionConfiguration.name, this)
}

tasks.buildPlugin {
    // Disable the IntelliJ plugin's default buildPlugin behavior for ZIP creation
    // and make it use our two-phase approach instead
    actions.clear()

    // Make it depend on our assembly task
    dependsOn(preparePluginForSigning)
    dependsOn(assemblePlugin)

    // Phase 2 (assemblePlugin + all its dependencies) must run after Phase 1
    assemblePlugin.get().mustRunAfter(preparePluginForSigning)
    assemblePlugin.get().taskDependencies.getDependencies(assemblePlugin.get()).forEach { task ->
        task.mustRunAfter(preparePluginForSigning)
    }
}

val riderModel: Configuration by configurations.creating {
    isCanBeConsumed = true
    isCanBeResolved = false
}

artifacts {
    add(riderModel.name, provider {
        intellijPlatform.platformPath.resolve("lib/rd/rider-model.jar").also {
            check(it.isFile) {
                "rider-model.jar is not found at $riderModel"
            }
        }
    }) {
        builtBy(Constants.Tasks.INITIALIZE_INTELLIJ_PLATFORM_PLUGIN)
    }
}

fun File.writeTextIfChanged(content: String) {
    val bytes = content.toByteArray()

    if (!exists() || !readBytes().contentEquals(bytes)) {
        println("Writing $path")
        parentFile.mkdirs()
        writeBytes(bytes)
    }
}
