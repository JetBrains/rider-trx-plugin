package com.jetbrains.rider.plugins.trxplugin.test.cases

import com.jetbrains.rider.plugins.trxplugin.TrxImportService
import com.jetbrains.rider.protocol.protocol
import com.jetbrains.rider.test.asserts.shouldBe
import com.jetbrains.rider.test.asserts.shouldBeTrue
import com.jetbrains.rider.test.asserts.shouldNotBeNull
import com.jetbrains.rider.test.base.BaseTestWithSolution
import com.jetbrains.rider.test.scriptingApi.runBlockingWithProtocolPumping
import com.jetbrains.rider.test.scriptingApi.withUtFacade
import org.testng.annotations.Test
import java.nio.file.Path
import kotlin.io.path.Path
import kotlin.io.path.absolute
import kotlin.io.path.isRegularFile

class BasicTrxImportTest : BaseTestWithSolution() {
    override val testSolution = "EmptySolution"

    @Test
    fun test1TrxImport() {
        val file = repositoryRoot.value.resolve("src/dotnet/Tests/TestData/test1.trx")
        file.isRegularFile().shouldBeTrue("File \"$file\" should exist.")

        val service = TrxImportService.getInstance(project)
        val response = runBlockingWithProtocolPumping(project.protocol, "BasicTrxImportTest") {
            service.importTrx(file.toString())
        }
        response.result.shouldBe("Success")
        withUtFacade(project) { ut ->
            val descriptor = ut.waitForAnySession()
            descriptor.title.shouldBe("test1.trx")

            ut.compareSessionTreeWithGold(descriptor, testGoldFile)
        }
    }
}

private val repositoryRoot = lazy {
    val cwd = Path(".").absolute()
    var current: Path? = cwd
    while (current != null && !current.resolve("settings.gradle.kts").isRegularFile()) {
        current = current.parent
    }
    return@lazy current.shouldNotBeNull("repository root not found on top of \"$cwd\".")
}
