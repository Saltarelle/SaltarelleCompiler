1. Download zipped jQueryUI Api documentation files from 
   https://github.com/jquery/api.jqueryui.com/zipball/master
2. Unzip in directory of your choice
3. Run the jQueryUIGenerator with specifying the source directory as first argument 
   and the destination directory as second argument and optionaly "/p" to generate 
   c# project file.
   
   Note: The source directory for the Generator is "[your unzipped files location]\entries".
   The destination directory can be directly "scriptsharp\src\Libraries\jQuery\jQuery.UI"

   Example:
   jQueryUIGenerator.exe "[your unzipped files location]\entries" "[destination directory]" /p