/// <reference path="jquery-1.7.1-vsdoc.js" />

$(function() {
    function addDownloadButton() {

        //single player
        $(".listenContent .sound.single").not(".playlist").not(".sdownload-added").each(function() {

            var self = $(this);
            self.addClass("sdownload-added");

            var buttonGroup = self.find(".sc-button-group").first();
            var downloadButton = buttonGroup.find(".scDownloaderButton").get(0);

            if (!downloadButton) {
                downloadButton = $("<button>").addClass("sc-button").addClass("sc-button-medium").addClass("sc-button-responsive").addClass("sc-button-download");
                downloadButton.addClass("scDownloaderButton");
                downloadButton.html("SDownload");

                var url = window.location.href.split("?")[0];

                downloadButton.attr("data-href", url);

                downloadButton.appendTo(buttonGroup);

                downloadButton.on("click", function(event) {
                    event.stopPropagation();
                    downloadClick(event.target);
                });
            }

        });

        //list
        $(".sound").not(".sdownload-added").each(function() {

            var self = $(this);
            self.addClass("sdownload-added");

            var buttonGroup = self.find(".sc-button-group").first();
            
            var downloadButton = buttonGroup.find(".scAppDownload").get(0);

            if (!downloadButton) {
                var downloadButton = $("<button>").addClass("sc-button").addClass("sc-button-small").addClass("sc-button-responsive").addClass("sc-button-download");
                downloadButton.addClass("scDownloaderButton");
                downloadButton.html("SDownload");

                var artCoverLink = self.find(".sound__coverArt").first();
                var url = artCoverLink.attr("href");
                url = "https://soundcloud.com" + url;

                downloadButton.attr("data-href", url);

                downloadButton.appendTo(buttonGroup);

                downloadButton.on("click", function(event) {
                    event.stopPropagation();
                    downloadClick(event.target);
                });
            }

        });


        function downloadClick(target) {

            var songHref = $(target).attr("data-href");

            $(target).off("click");

            if (songHref && songHref.length > 0) {
                $(target).html("Sent to Application!");
                window.location = "sdownload://" + songHref;
            }
        }
    }

    setInterval(function() {
        addDownloadButton();
    }, 100);

});