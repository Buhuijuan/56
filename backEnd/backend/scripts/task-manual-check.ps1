param(
    [ValidateSet("m1_1", "chapter1", "branch1", "full", "negative")]
    [string]$Step = "full",
    [string]$BaseUrl = "http://localhost:8080"
)

$ErrorActionPreference = "Stop"

function Write-Section([string]$title) {
    Write-Host ""
    Write-Host "==== $title ====" -ForegroundColor Cyan
}

function Invoke-JsonPost([string]$url, [hashtable]$body, $headers = $null) {
    $json = $body | ConvertTo-Json -Depth 10
    if ($headers) {
        return Invoke-RestMethod -Uri $url -Method Post -Headers $headers -ContentType "application/json" -Body $json
    }
    return Invoke-RestMethod -Uri $url -Method Post -ContentType "application/json" -Body $json
}

function Invoke-JsonGet([string]$url, $headers) {
    return Invoke-RestMethod -Uri $url -Method Get -Headers $headers
}

function Show-CurrentMain($headers) {
    $current = Invoke-JsonGet "$BaseUrl/api/tasks/current/main" $headers
    $task = $current.data.task
    if ($null -eq $task) {
        Write-Host "当前主线: null" -ForegroundColor Yellow
        return $null
    }
    Write-Host ("当前主线: {0} | {1} | {2} | 进度 {3}/{4}" -f $task.taskCode, $task.taskName, $task.status, $task.progressCurrent, $task.progressTarget) -ForegroundColor Green
    return $task
}

function Show-Task($headers, [string]$taskCode) {
    $task = Invoke-JsonGet "$BaseUrl/api/tasks/$taskCode" $headers
    Write-Host ("任务 {0}: {1} | 状态 {2} | 进度 {3}/{4}" -f $task.data.taskCode, $task.data.taskName, $task.data.status, $task.data.progressCurrent, $task.data.progressTarget)
    return $task.data
}

function Send-TaskEvent($headers, [hashtable]$body) {
    Write-Host ("发送事件: {0}" -f (($body | ConvertTo-Json -Compress))) -ForegroundColor DarkGray
    $result = Invoke-JsonPost "$BaseUrl/api/tasks/events" $body $headers
    Write-Host ("事件结果: triggered={0} progressed={1}" -f (($result.data.triggeredTasks -join ",")), ($result.data.progressedTasks.Count)) -ForegroundColor DarkYellow
    return $result
}

function Claim-Task($headers, [string]$taskCode) {
    $result = Invoke-JsonPost "$BaseUrl/api/tasks/$taskCode/claim" @{} $headers
    $rewards = $result.data.rewards | ConvertTo-Json -Compress
    Write-Host ("领取 {0}: {1}" -f $taskCode, $rewards) -ForegroundColor Magenta
    return $result
}

function Accept-Task($headers, [string]$taskCode) {
    $result = Invoke-JsonPost "$BaseUrl/api/tasks/$taskCode/accept" @{} $headers
    Write-Host ("接取 {0}: accepted={1}" -f $taskCode, $result.data.accepted) -ForegroundColor Magenta
    return $result
}

function Show-Chapters($headers) {
    $chapters = Invoke-JsonGet "$BaseUrl/api/tasks/chapters" $headers
    foreach ($chapter in $chapters.data.chapters) {
        Write-Host ("章节 {0}: 完成 {1}/{2} | 已领奖 {3} | chapterCompleted={4}" -f $chapter.chapterNo, $chapter.completedCount, $chapter.taskCount, $chapter.claimedCount, $chapter.chapterCompleted)
    }
}

function Show-Home($headers) {
    $home = Invoke-JsonGet "$BaseUrl/api/game/home" $headers
    $profile = $home.data.profile
    Write-Host ("角色成长: level={0} exp={1} coin={2} bikeUnlocked={3}" -f $profile.level, $profile.exp, $profile.coin, $profile.bikeUnlocked) -ForegroundColor Green
}

function New-TestSession {
    Write-Section "注册登录并创建角色"
    $mailbox = "task-manual-$([guid]::NewGuid().ToString('N').Substring(0, 8))@example.com"
    $sendCode = Invoke-JsonPost "$BaseUrl/api/auth/send-code" @{ mailbox = $mailbox }
    $code = $sendCode.data.verificationCode
    Write-Host "验证码: $code"

    Invoke-JsonPost "$BaseUrl/api/auth/register" @{
        mailbox = $mailbox
        password = "Password123"
        verificationCode = $code
    } | Out-Null

    $login = Invoke-JsonPost "$BaseUrl/api/auth/login" @{
        mailbox = $mailbox
        password = "Password123"
    }

    $token = $login.data.token
    $headers = @{ Authorization = "Bearer $token" }

    Invoke-JsonPost "$BaseUrl/api/player/roles" @{
        campusName = "此间校园"
        nickName = "任务测试"
        characterId = 1
    } $headers | Out-Null

    return $headers
}

function Run-M1_1($headers) {
    Write-Section "M_1_1"
    Show-CurrentMain $headers | Out-Null
    Send-TaskEvent $headers @{ eventType = "AI_DIALOGUE"; success = $true } | Out-Null
    Show-Task $headers "M_1_1" | Out-Null
    Claim-Task $headers "M_1_1" | Out-Null
    Show-CurrentMain $headers | Out-Null
}

function Run-Chapter1($headers) {
    Run-M1_1 $headers

    Write-Section "M_1_2"
    Send-TaskEvent $headers @{ eventType = "ARRIVE_BUILDING"; targetType = "arrive_building"; targetId = 2001; success = $true } | Out-Null
    Claim-Task $headers "M_1_2" | Out-Null
    Show-CurrentMain $headers | Out-Null

    Write-Section "M_1_3"
    Send-TaskEvent $headers @{ eventType = "NPC_DIALOGUE"; targetType = "npc_dialogue"; targetId = 1003; success = $true } | Out-Null
    Claim-Task $headers "M_1_3" | Out-Null
    Show-CurrentMain $headers | Out-Null

    Write-Section "M_1_4"
    Send-TaskEvent $headers @{ eventType = "NPC_DIALOGUE"; targetType = "npc_dialogue"; targetId = 1004; success = $true } | Out-Null
    Claim-Task $headers "M_1_4" | Out-Null
    Show-Chapters $headers
    Show-CurrentMain $headers | Out-Null
}

function Run-Branch1($headers) {
    Run-Chapter1 $headers

    Write-Section "触发 B_1_1"
    Send-TaskEvent $headers @{ eventType = "LANDMARK_VISIT"; targetId = 2004; success = $true } | Out-Null
    Show-Task $headers "B_1_1" | Out-Null

    Write-Section "推进 B_1_1"
    Send-TaskEvent $headers @{
        eventType = "PHOTO_CHECKIN"
        targetId = 2004
        currentPosX = 0
        currentPosY = 0
        currentPosZ = 0
    } | Out-Null
    Send-TaskEvent $headers @{
        eventType = "PHOTO_CHECKIN"
        targetId = 2006
        currentPosX = 0
        currentPosY = 0
        currentPosZ = 0
    } | Out-Null
    Send-TaskEvent $headers @{
        eventType = "PHOTO_CHECKIN"
        targetId = 2011
        currentPosX = 0
        currentPosY = 0
        currentPosZ = 0
    } | Out-Null
    Show-Task $headers "B_1_1" | Out-Null
    Claim-Task $headers "B_1_1" | Out-Null

    Write-Section "触发并完成 B_2_1"
    Send-TaskEvent $headers @{ eventType = "BIKE_STATION_VISIT"; targetId = 4001; success = $true } | Out-Null
    Show-Task $headers "B_2_1" | Out-Null
    Send-TaskEvent $headers @{ eventType = "BIKE_TRIAL_DISTANCE"; targetId = 4001; increment = 50; success = $true } | Out-Null
    Claim-Task $headers "B_2_1" | Out-Null
    Show-Home $headers
}

function Run-Full($headers) {
    Run-Branch1 $headers

    Write-Section "第二章"
    Accept-Task $headers "M_2_1" | Out-Null
    Send-TaskEvent $headers @{ eventType = "ARRIVE_BUILDING"; targetId = 2004; success = $true } | Out-Null
    Claim-Task $headers "M_2_1" | Out-Null
    Send-TaskEvent $headers @{ eventType = "ARRIVE_BUILDING"; targetId = 2005; success = $true } | Out-Null
    Claim-Task $headers "M_2_2" | Out-Null
    Send-TaskEvent $headers @{ eventType = "ARRIVE_BUILDING"; targetId = 2006; success = $true } | Out-Null
    Claim-Task $headers "M_2_3" | Out-Null

    Write-Section "第三章"
    Accept-Task $headers "M_3_1" | Out-Null
    Send-TaskEvent $headers @{ eventType = "ARRIVE_BUILDING"; targetId = 2007; success = $true } | Out-Null
    Claim-Task $headers "M_3_1" | Out-Null
    Send-TaskEvent $headers @{ eventType = "ARRIVE_BUILDING"; targetId = 2008; success = $true } | Out-Null
    Claim-Task $headers "M_3_2" | Out-Null
    Send-TaskEvent $headers @{ eventType = "ARRIVE_BUILDING"; targetId = 2009; success = $true } | Out-Null
    Claim-Task $headers "M_3_3" | Out-Null
    Send-TaskEvent $headers @{ eventType = "ARRIVE_BUILDING"; targetId = 2010; success = $true } | Out-Null
    Claim-Task $headers "M_3_4" | Out-Null

    Write-Section "最终状态"
    Invoke-JsonGet "$BaseUrl/api/tasks" $headers | ConvertTo-Json -Depth 8
    Show-CurrentMain $headers | Out-Null
    Show-Home $headers
}

function Run-Negative($headers) {
    Write-Section "重复领奖应失败"
    try {
        Claim-Task $headers "M_1_1" | Out-Null
        Write-Host "重复领奖未失败，请检查逻辑" -ForegroundColor Red
    } catch {
        Write-Host ("重复领奖失败符合预期: {0}" -f $_.Exception.Message) -ForegroundColor Yellow
    }

    Write-Section "未满足前置条件接取后续任务应失败"
    try {
        Accept-Task $headers "M_3_1" | Out-Null
        Write-Host "越级接取未失败，请检查逻辑" -ForegroundColor Red
    } catch {
        Write-Host ("越级接取失败符合预期: {0}" -f $_.Exception.Message) -ForegroundColor Yellow
    }
}

$headers = New-TestSession

switch ($Step) {
    "m1_1" { Run-M1_1 $headers }
    "chapter1" { Run-Chapter1 $headers }
    "branch1" { Run-Branch1 $headers }
    "full" { Run-Full $headers }
    "negative" {
        Run-Chapter1 $headers
        Send-TaskEvent $headers @{ eventType = "LANDMARK_VISIT"; targetId = 2004; success = $true } | Out-Null
        try {
            Send-TaskEvent $headers @{
                eventType = "PHOTO_CHECKIN"
                targetId = 2004
                currentPosX = 500
                currentPosY = 0
                currentPosZ = 0
            } | Out-Null
            Write-Host "远距离打卡未失败，请检查逻辑" -ForegroundColor Red
        } catch {
            Write-Host ("远距离打卡失败符合预期: {0}" -f $_.Exception.Message) -ForegroundColor Yellow
        }
        Run-Negative $headers
    }
}
