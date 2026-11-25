param([string]$PreviewFramesPath)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$portraitPath = Join-Path $PSScriptRoot '../../../unityproj/ImperiumOfMan/Assets/ImperiumOfManMod/Portraits/ChaosUndividedComms.png'
$atlasPath = Join-Path $PSScriptRoot 'ChaosUndividedCommsEye.png'
$eyeLeft = 66
$eyeTop = 81
$eyeWidth = 99
$eyeHeight = 53
$frameCount = 120
$columns = 10
$pupilCenter = 113.5
$eyeCenterY = 106.0

function Sample-Track([double[]]$keys) {
    $samples = [double[]]::new($frameCount)
    $key = 0
    for ($frame = 0; $frame -lt $frameCount; $frame++) {
        if ($frame -gt $keys[$key + 2]) { $key += 2 }
        $offset = ($frame - $keys[$key]) / ($keys[$key + 2] - $keys[$key])
        $samples[$frame] = $keys[$key + 1] + $offset * ($keys[$key + 3] - $keys[$key + 1])
    }
    ,$samples
}

# Keyframes are given in frames at 20 fps. Uneven holds and a double blink keep the motion irregular.
$gaze = Sample-Track @(0,0, 10,0, 12,-19, 34,-19, 36,-14, 42,-14, 44,17, 55,17, 57,20, 74,20, 76,-19, 89,-19, 91,6, 103,6, 106,0, 119,0)
$openness = Sample-Track @(0,1, 26,1, 28,0, 30,0, 33,1, 62,1, 64,0, 66,0, 69,1, 70,1, 72,0, 73,0, 76,1, 119,1)

if ($PreviewFramesPath) { New-Item -ItemType Directory -Path $PreviewFramesPath -Force | Out-Null }
$portrait = [Drawing.Bitmap]::new($portraitPath)
$atlas = [Drawing.Bitmap]::new($columns * $eyeWidth, ($frameCount / $columns) * $eyeHeight, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
try {
    if ($portrait.Width -ne 228 -or $portrait.Height -ne 228) { throw 'The animation expects a 228-pixel portrait.' }
    $basePixels = [Drawing.Color[,]]::new($eyeWidth, $eyeHeight)
    $left = [int[]]::new($eyeHeight)
    $right = [int[]]::new($eyeHeight)
    $top = [int[]]::new($eyeWidth)
    $bottom = [int[]]::new($eyeWidth)
    [Array]::Fill($left, $eyeWidth)
    [Array]::Fill($right, -1)
    [Array]::Fill($top, $eyeHeight)
    [Array]::Fill($bottom, -1)

    for ($row = 0; $row -lt $eyeHeight; $row++) {
        $y = $eyeTop + $row
        # Limit the luminance search to the eye so the surrounding sigil stays fixed.
        $halfWidth = 47.0 - 1.78 * [Math]::Abs($y - $eyeCenterY)
        for ($column = 0; $column -lt $eyeWidth; $column++) {
            $x = $eyeLeft + $column
            $color = $portrait.GetPixel($x, $y)
            $basePixels[$column, $row] = $color
            if ([Math]::Abs($x - $pupilCenter) -le $halfWidth -and $color.G -ge 129 -and $color.R -ge 70) {
                $left[$row] = [Math]::Min($left[$row], $column)
                $right[$row] = $column
            }
        }
        for ($column = $left[$row]; $column -le $right[$row]; $column++) {
            $top[$column] = [Math]::Min($top[$column], $row)
            $bottom[$column] = $row
        }
    }

    $center = $pupilCenter - $eyeLeft
    for ($frame = 0; $frame -lt $frameCount; $frame++) {
        $atlasX = ($frame % $columns) * $eyeWidth
        $atlasY = [Math]::Floor($frame / $columns) * $eyeHeight
        $preview = if ($PreviewFramesPath) { $portrait.Clone() }
        try {
            for ($row = 0; $row -lt $eyeHeight; $row++) {
                $span = $right[$row] - $left[$row]
                $shiftedPupil = $center + $gaze[$frame] * [Math]::Min(1.0, $span / 81.0)
                for ($column = 0; $column -lt $eyeWidth; $column++) {
                    $color = $basePixels[$column, $row]
                    if ($span -gt 6 -and $column -gt $left[$row] -and $column -lt $right[$row]) {
                        if ($gaze[$frame] -ne 0.0) {
                            if ($column -lt $shiftedPupil) {
                                $sampleX = $left[$row] + ($column - $left[$row]) * ($center - $left[$row]) / ($shiftedPupil - $left[$row])
                            } else {
                                $sampleX = $right[$row] + ($column - $right[$row]) * ($right[$row] - $center) / ($right[$row] - $shiftedPupil)
                            }
                            $sampleX = [Math]::Clamp($sampleX, [double]$left[$row], [double]$right[$row])
                            $near = [int][Math]::Floor($sampleX)
                            $mix = $sampleX - $near
                            $a = $basePixels[$near, $row]
                            $b = $basePixels[[Math]::Min($near + 1, $right[$row]), $row]
                            $color = [Drawing.Color]::FromArgb(255, [int]($a.R + ($b.R - $a.R) * $mix), [int]($a.G + ($b.G - $a.G) * $mix), [int]($a.B + ($b.B - $a.B) * $mix))
                        }

                    }
                    if ($openness[$frame] -lt 1.0 -and $span -gt 0 -and $column -ge $left[$row] - 2 -and $column -le $right[$row] + 2) {
                        $seam = $eyeCenterY - $eyeTop + 0.018 * ($column - $center)
                        $upperLid = $seam + ($top[$column] - $seam) * $openness[$frame]
                        $lowerLid = $seam + ($bottom[$column] - $seam) * $openness[$frame]
                        $inside = [Math]::Min($row - $upperLid, $lowerLid - $row)
                        $edge = [Math]::Min($column - $left[$row] + 2, $right[$row] - $column + 2)
                        $coverage = [Math]::Clamp($edge / 2.0, 0.0, 1.0)
                        $shadow = $coverage * (1.0 - [Math]::Clamp($inside / 1.9, 0.0, 1.0))
                        $seamGlow = $coverage * 62.0 * (1.0 - $openness[$frame]) * [Math]::Max(0.0, 1.0 - [Math]::Abs($row - $seam))
                        $lidGreen = 15.0 + 2.0 * ($row % 2)
                        $color = [Drawing.Color]::FromArgb(255,
                            [int][Math]::Min(255.0, $color.R * (1.0 - $shadow) + 5.0 * $shadow + 0.35 * $seamGlow),
                            [int][Math]::Min(255.0, $color.G * (1.0 - $shadow) + $lidGreen * $shadow + $seamGlow),
                            [int][Math]::Min(255.0, $color.B * (1.0 - $shadow) + 12.0 * $shadow + 0.53 * $seamGlow))
                    }
                    $atlas.SetPixel(($atlasX + $column), ($atlasY + $row), $color)
                    if ($preview) { $preview.SetPixel(($eyeLeft + $column), ($eyeTop + $row), $color) }
                }
            }
            if ($preview) { $preview.Save((Join-Path $PreviewFramesPath ('eye-{0:D3}.png' -f $frame)), [Drawing.Imaging.ImageFormat]::Png) }
        } finally {
            if ($preview) { $preview.Dispose() }
        }
    }
    $atlas.Save($atlasPath, [Drawing.Imaging.ImageFormat]::Png)
} finally {
    $atlas.Dispose()
    $portrait.Dispose()
}
