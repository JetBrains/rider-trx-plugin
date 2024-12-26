package com.jetbrains.rider.plugins.trxplugin

import com.intellij.openapi.components.Service
import com.intellij.openapi.components.service
import com.intellij.openapi.project.Project
import kotlinx.coroutines.CoroutineScope

val Project.coroutineScope: CoroutineScope
    get() = service<ScopeHolder>().scope

@Service(Service.Level.PROJECT)
private class ScopeHolder(val scope: CoroutineScope)
