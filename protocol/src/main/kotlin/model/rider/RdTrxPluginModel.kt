package model.rider

import com.jetbrains.rd.generator.nova.*
import com.jetbrains.rd.generator.nova.PredefinedType.string
import com.jetbrains.rd.generator.nova.csharp.CSharp50Generator
import com.jetbrains.rd.generator.nova.kotlin.Kotlin11Generator
import com.jetbrains.rider.model.nova.ide.SolutionModel

@Suppress("unused")
object RdTrxPluginModel : Ext(SolutionModel.Solution) {
    private val RdCallRequest = structdef {
        field("trxPath", string)
    }

    private val RdCallResponse = structdef {
        field("result", string)
        field("message", string)
    }

    init {
        setting(Kotlin11Generator.Namespace, "com.jetbrains.rider.plugins.trxplugin.model")
        setting(CSharp50Generator.Namespace, "Rider.Plugins.TrxPlugin.Model")

        call("importTrxCall", RdCallRequest, RdCallResponse)
    }
}
