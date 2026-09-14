$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$nunit = Join-Path $projectRoot 'Library/PackageCache/com.unity.ext.nunit/net40/unity-custom/nunit.framework.dll'
Add-Type -Path $nunit
$sources = @(
    (Join-Path $projectRoot 'Assets/Scripts/Framework/Animation/OneShotTimeline.cs'),
    (Join-Path $projectRoot 'Assets/Scripts/Game/Bosses/Potato/PotatoAttackSequence.cs'),
    (Join-Path $PSScriptRoot 'PlayMode/PotatoAttackTimelineTests.cs')
)
Add-Type -Path $sources -ReferencedAssemblies @($nunit, (Join-Path $PSHOME 'ref/mscorlib.dll')) -CompilerOptions '/define:UNITY_INCLUDE_TESTS'
$tests = [MMORPG.Tests.PlayMode.PotatoAttackTimelineTests]::new()
$tests.PauseAndBoundaryDoNotRepeatMarker()
$tests.CancelBeforeReleaseAndRestartLeaveNoOldEvents()
$tests.HugeFramesKeepThreeShotsAndFullFinalRecovery()
foreach ($fps in @(30, 60, 120)) { $tests.NormalFramesRemainMonotonicAndReleaseExactlyThreeTimes($fps) }
$tests.InvalidMarkersAreRejected()
$tests.ZeroTellAndRecoveryPreserveReleases(0, 0.4, 3)
$tests.ZeroTellAndRecoveryPreserveReleases(0.5, 0, 3)
$tests.ZeroTellAndRecoveryPreserveReleases(0, 0, 3)
$tests.ZeroTellAndRecoveryPreserveReleases(0, 0, 1)
Write-Output '土豆时间轴纯逻辑测试通过：11/11；未启动 Unity。'
