package com.jetbrains.rider.plugins.trxplugin

import com.intellij.openapi.components.Service
import com.intellij.openapi.components.service
import com.intellij.openapi.project.Project
import com.jetbrains.rider.plugins.trxplugin.model.RdCallRequest
import com.jetbrains.rider.plugins.trxplugin.model.RdCallResponse
import com.jetbrains.rider.plugins.trxplugin.model.rdTrxPluginModel
import com.jetbrains.rider.projectView.solution

@Service(Service.Level.PROJECT)
class TrxImportService(private val project: Project) {

    companion object {
        fun getInstance(project: Project) = project.service<TrxImportService>()
    }

    suspend fun importTrx(input: String): RdCallResponse {
        val model = project.solution.rdTrxPluginModel
        val request = RdCallRequest(input)
        val response = model.importTrxCall.startSuspending(request)
        return response
    }
}
