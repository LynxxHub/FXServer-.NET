fx_version 'bodacious'
game 'gta5'

-- Specify the path to the HTML file that will be used as the UI
ui_page 'wwwroot/HTML/index.html'

-- List all files in the wwwroot folder that should be included with your resource
files {
    'wwwroot/HTML/index.html',
    'wwwroot/CSS/*.css',
    'wwwroot/JS/*.js',
    'Client/bin/Release/**/publish/Newtonsoft.Json.dll',
}

client_script 'Client/bin/Release/**/publish/*.net.dll'
server_script 'Server/bin/Release/**/publish/*.net.dll'

author 'Lynx'
version '1.0.0'
description 'Still dont know xD'