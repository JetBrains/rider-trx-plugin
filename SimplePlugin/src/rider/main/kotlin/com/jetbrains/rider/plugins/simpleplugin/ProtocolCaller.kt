package com.jetbrains.rider.plugins.simpleplugin

import com.intellij.openapi.components.Service
import com.intellij.openapi.project.Project
import com.jetbrains.rider.plugins.simpleplugin.model.RdCallRequest
import com.jetbrains.rider.plugins.simpleplugin.model.rdSimplePluginModel
import com.jetbrains.rider.projectView.solution

@Service(Service.Level.PROJECT)
class ProtocolCaller(private val project: Project) {

    suspend fun doCall(input: String): String {
        val model = project.solution.rdSimplePluginModel
        val request = RdCallRequest(input)
        val response = model.myCall.startSuspending(request)
        return response.myResult
    }
}
