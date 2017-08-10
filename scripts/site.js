$(function()
{
    $("table").addClass("table table-bordered table-striped table-condensed");
    $("blockquote").addClass("blockquote");

    // Download Link
    $.getJSON("https://api.github.com/repos/MathewSachin/Captura/releases/latest").done(function(release) {
        for (var j = 0; j < release.assets.length; ++j)
        {
            var asset = release.assets[j];

            if (asset.name == "Setup.exe")
            {
                $(".download-link").attr("href", asset.browser_download_url);

                break;
            }
        }
    });
})