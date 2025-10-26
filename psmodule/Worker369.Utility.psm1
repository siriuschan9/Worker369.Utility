$_my_settings_params = @{
    Name        = 'my_settings'
    Value       = [Worker369.Utility.MySettings]::Instance
    Option      = [System.Management.Automation.ScopedItemOptions]::ReadOnly
    Scope       = 'Script'
    Description = 'All format settings.'
}

$_number_info_params = @{
    Name        = 'number_info_settings'
    Value       = [Worker369.Utility.NumberInfoSettings]::Instance
    Option      = [System.Management.Automation.ScopedItemOptions]::ReadOnly
    Scope       = 'Script'
    Description = 'NumberInfo format settings.'
}

$_byte_info_params = @{
    Name        = 'byte_info_settings'
    Value       = [Worker369.Utility.ByteInfoSettings]::Instance
    Option      = [System.Management.Automation.ScopedItemOptions]::ReadOnly
    Scope       = 'Script'
    Description = 'ByteInfo format settings.'
}

$_ipv4_address_params = @{
    Name        = 'ipv4_address_settings'
    Value       = [Worker369.Utility.IPv4AddressSettings]::Instance
    Option      = [System.Management.Automation.ScopedItemOptions]::ReadOnly
    Scope       = 'Script'
    Description = 'IPv4Address format settings.'
}

$_ipv6_address_params = @{
    Name        = 'ipv6_address_settings'
    Value       = [Worker369.Utility.IPv6AddressSettings]::Instance
    Option      = [System.Management.Automation.ScopedItemOptions]::ReadOnly
    Scope       = 'Script'
    Description = 'IPv6Address format settings.'
}

$_format_column_params = @{
    Name        = 'format_column_settings'
    Value       = [Worker369.Utility.FormatColumnSettings]::Instance
    Option      = [System.Management.Automation.ScopedItemOptions]::ReadOnly
    Scope       = 'Script'
    Description = 'Format-Column format settings.'
}

Set-Variable @_my_settings_params
Set-Variable @_byte_info_params
Set-Variable @_number_info_params
Set-Variable @_ipv4_address_params
Set-Variable @_ipv6_address_params
Set-Variable @_format_column_params

# Once we call Export-ModuleMember, powershell stops the default "export everything".
# We must explicitly specify to export cmdlets and aliases.
Export-ModuleMember -Variable @(
    'my_settings',
    'byte_info_settings',
    'number_info_settings',
    'ipv4_address_settings',
    'ipv6_address_settings',
    'format_column_settings'
)

Export-ModuleMember -Cmdlet *
Export-ModuleMember -Alias *