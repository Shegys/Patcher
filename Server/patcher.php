<?php
function dir_rekursiv($verzeichnis)
{ 
    $handle =  opendir($verzeichnis);
    while ($datei = readdir($handle))
    {
        if ($datei != "." && $datei != "..")
        {
            if (is_dir($verzeichnis.$datei))
            {
                dir_rekursiv($verzeichnis.$datei.'/'); 
            } else {
                $return = preg_replace("|client/|", "", $verzeichnis.$datei);
                $return = preg_replace("|/|", "\\", $return);
                echo $return."|".md5_file($verzeichnis.$datei)."\r\n";
            }
        }
    }
    closedir($handle);
}

function dirrek($verzeichnis)
{ 
    $handle =  opendir($verzeichnis);
    while ($datei = readdir($handle))
    {
        if ($datei != "." && $datei != "..")
        {
            if (is_dir($verzeichnis.$datei))
            {
                $return = preg_replace("|client/|", "", $verzeichnis.$datei);
                echo $return."/\n";
                dirrek($verzeichnis.$datei.'/');
            }
        }
    }
    closedir($handle);
}
?>
[DIRECTORY]
<?php
dirrek("client/");
?>
[/DIRECTORY]
<?php
dir_rekursiv('client/');
?>