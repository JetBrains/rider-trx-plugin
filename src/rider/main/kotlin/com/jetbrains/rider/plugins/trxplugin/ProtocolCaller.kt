package com.jetbrains.rider.plugins.trxplugin

import com.intellij.openapi.components.Service
import com.intellij.openapi.project.Project
import com.jetbrains.rider.plugins.trxplugin.model.RdCallRequest
import com.jetbrains.rider.plugins.trxplugin.model.rdTrxPluginModel
import com.jetbrains.rider.projectView.solution

@Service(Service.Level.PROJECT)
class ProtocolCaller(private val project: Project) {

    suspend fun doCall(input: String): String {
        val model = project.solution.rdTrxPluginModel
        val request = RdCallRequest(input)
        val response = model.myCall.startSuspending(request)
        return response.myResult
    }
}
