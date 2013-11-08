Framework "4.0x86"

Task Build-Master {
	Invoke-Psake default.ps1 -properties @{autoVersion = $false; noAsync = $true; configuration = "Release" }
}

Task Build-Develop {
	Invoke-Psake default.ps1 -properties @{noAsync = $true; configuration = "Release" }
}