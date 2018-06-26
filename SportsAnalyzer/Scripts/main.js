var varTitleFontSize, ticksFontSize, legendFontSize, tooltipsFontSize, labelsFontSize;
var timeIntervalsTexts = [];
var goalsInIntervalsPercent;
var selectedRounds = [];
var webApiUri = 'api/stats';
var goalsIntervalsURI = webApiUri + "/goalsintervals";
var goalsInIntervalsTitle = "Minutes Intervales of scored goals"
var goalsInIntervalsXLabel = "Time intervals [min.]";
var goalsInIntervalsYLabel = "Percent of goals";
var matchGoalsURI = webApiUri + "/matchgoals";
var matchGoalsTitle = "Percent of Matches with a given number of goals";
var matchGoalsXLabel = "Number of goals";
var matchGoalsYLabel = "Percent of matches";
var leagueName, seasonYear;

var myChartColors = [window.chartColors.blue,
window.chartColors.red,
window.chartColors.orange,
window.chartColors.yellow];

// Getting data of the Model passed from the Stats view

timeIntervalsTexts = eval($("#mainScript").attr("data-time-intervals-all-text"));
goalsInIntervalsPercent = eval($("#mainScript").attr("data-goals-in-intervals-percent"));
leagueName = eval($("#mainScript").attr("data-league-name"));
seasonYear = eval($("#mainScript").attr("data-season-year"));

function RemoveChartDataset(chart, teamName) {
  for (var i = 0; i < chart.data.datasets.length; i++) {
    if (chart.data.datasets[i].label === teamName) {
      chart.data.datasets.splice(i, 1);
    }
  }
  chart.update();
}

function UpdateChart(chart, chartDisplaySize) {
  UpdateChartFontSizes(chart, chartDisplaySize);
  chart.update();
}

function ConfirmSelectedRounds() {
  var roundsSize = selectedRounds.length;
  for (var i = 0; i < roundsSize; i++) {
    selectedRounds.pop();
  }
  $("#roundsList :selected").each(function () {
    selectedRounds.push($(this).val());
  });
}

function GetStatsRequestData(teamName) {
  return {
    "TeamName": teamName,
    "LeagueName": leagueName,
    "SeasonYear": seasonYear,
    "Rounds": selectedRounds
  };
}

function AddChartDataset(chart, URI, teamName, id) {
  var statsRequestData = GetStatsRequestData(teamName);
  $.post(URI, statsRequestData, null, "json")
    .done(function (data) {
      var newData;
      if (URI == matchGoalsURI) {
        let chartData;
        chartData = GetMatchGoalsData(data);
        newData = chartData.data;
      }
      else {
        newData = data;
      }

      var dataset = {
        label: teamName,
        backgroundColor: color(myChartColors[id]).alpha(0.5).rgbString(),
        borderColor: myChartColors[id],
        borderWidth: 1,
        data: newData
      };

      // calling RemoveChartDataset method in case of delay in receiving results from WebApi
      RemoveChartDataset(chart, teamName);
      chart.data.datasets.push(dataset);
      chart.update();
    })
    .fail(function (jqXHR, textStatus, err) {
      console.log('Error: ' + err);
    });
}

function GetChartDisplaySize(chartName) {
  var canvasChart = document.getElementById(chartName);

  chartDisplaySize = {
    width: parseFloat(canvasChart.style.width),
    height: parseFloat(canvasChart.style.height)
  };
  return chartDisplaySize;
}

function UpdateChartFontSizes(chart, chartSize) {
  if (typeof chartSize !== "undefined") {
    legendFontSize = Math.round(0.01 * chartSize.width + 8);
    varTitleFontSize = Math.round(0.015 * chartSize.width + 8);
    tooltipsFontSize = Math.round(0.005 * chartSize.width + 10);
    labelsFontSize = Math.round(0.005 * chartSize.width + 10);
    ticksFontSize = Math.round(0.005 * chartSize.width + 10);
  }

  if (typeof chart !== "undefined") {
    chart.options.title.fontSize = varTitleFontSize;
    chart.options.legend.labels.fontSize = legendFontSize;
    chart.options.tooltips.titleFontSize = tooltipsFontSize;
    chart.options.tooltips.bodyFontSize = tooltipsFontSize;
    chart.options.scales.yAxes[0].scaleLabel.fontSize = labelsFontSize;
    chart.options.scales.xAxes[0].scaleLabel.fontSize = labelsFontSize;
    chart.options.scales.yAxes[0].ticks.fontSize = ticksFontSize;
    chart.options.scales.xAxes[0].ticks.fontSize = ticksFontSize;
  }
}

function PrepareChartData() {
  color = Chart.helpers.color;
}

function OnResizeChart(chart, chartSize) {
  UpdateChart(chart, chartSize);
}

function CreateChart(chartName, title, labels, data, xAxisLabel, yAxisLabel) {
  var ctx = $("#" + chartName);
  var chart = new Chart(ctx, {
    // The type of chart we want to create
    type: 'bar',
    // The data for our dataset
    data: {
      labels: labels,
      datasets: [{
        label: leagueName,
        backgroundColor: color(myChartColors[0]).alpha(0.5).rgbString(),
        borderColor: myChartColors[0],
        borderWidth: 1,
        data: data
      }]
    },
    // Configuration options go here
    options: {
      responsive: true,
      maintainAspectRatio: false,
      legend: {
        position: 'top',
        labels: {
          fontSize: legendFontSize,
          boxWidth: 30
        }
      },
      title: {
        display: true,
        fontSize: varTitleFontSize,
        fontColor: window.chartColors.black,
        text: title
      },
      tooltips: {
        titleFontSize: tooltipsFontSize,
        bodyFontSize: tooltipsFontSize,
        callbacks: {
          label: function (tooltipItem, data) {
            var label = data.datasets[tooltipItem.datasetIndex].label || '';

            if (label) {
              label += ': ';
            }
            label += tooltipItem.yLabel;
            label += "%";
            return label;
          }
        }
      },
      scales: {
        yAxes: [{
          scaleLabel: {
            display: true,
            labelString: yAxisLabel,
            fontColor: window.chartColors.blue,
            fontSize: labelsFontSize
          },
          ticks: {
            beginAtZero: true,
            fontSize: ticksFontSize,
            callback: function (value, index, values) {
              return value + '%';
            }
          }
        }],
        xAxes: [{
          scaleLabel: {
            display: true,
            labelString: xAxisLabel,
            fontColor: window.chartColors.blue,
            fontSize: labelsFontSize
          },
          ticks: {
            fontSize: ticksFontSize
          },
          gridLines: {
            offsetGridLines: true
          }
        }]
      }
    }
  });
  chart.options.onResize = OnResizeChart;

  chartDisplaySize = GetChartDisplaySize(chartName);
  UpdateChart(chart, chartDisplaySize);
  return chart;
}

$(document).ready(function () {
  PrepareChartData();
  window.goalsInIntervalsChart = CreateChart(
    "goalsInIntervalsChartArea",
    goalsInIntervalsTitle,
    timeIntervalsTexts,
    goalsInIntervalsPercent,
    goalsInIntervalsXLabel,
    goalsInIntervalsYLabel);

  ConfirmSelectedRounds();
  window.matchGoalsChart = CreateChart(
    "matchGoalsChartArea",
    matchGoalsTitle,
    [],
    [],
    matchGoalsXLabel,
    matchGoalsYLabel);

  GetMatchGoals(window.matchGoalsChart, "*", matchGoalsURI);
});

$(".form-check-input").change(function () {
  var teamName = $("label[for='" + $(this).attr('id') + "']").text();
  var id = $(this).val();

  if ($(this).prop("checked")) {
    AddChartDataset(window.goalsInIntervalsChart, goalsIntervalsURI, teamName, id, );
    AddChartDataset(window.matchGoalsChart, matchGoalsURI, teamName, id);
  }
  else {
    RemoveChartDataset(window.goalsInIntervalsChart, teamName);
    RemoveChartDataset(window.matchGoalsChart, teamName);
  }
});

function GetGoalsInIntervals(chart, URI, index, teamName) {
  var statsRequestData = GetStatsRequestData(teamName);
  $.post(URI, statsRequestData, null, "json")
    .done(function (data) {
      UpdateChartData(chart, data, index);
    })
    .fail(function (jqXHR, textStatus, err) {
      console.log('Error: ' + err);
    });
}

function UpdateChartData(chart, data, index) {
  chart.data.datasets[index].data = data;
  chart.update();
}

$("#changeRounds").click(function () {
  ConfirmSelectedRounds();

  for (var i = 0; i < window.goalsInIntervalsChart.data.datasets.length; i++) {
    var dataset = window.goalsInIntervalsChart.data.datasets[i];
    var teamName = "*";

    if (dataset.label !== leagueName) {
      teamName = dataset.label;
    }

    GetGoalsInIntervals(window.goalsInIntervalsChart, goalsIntervalsURI, i, teamName);
    GetMatchGoals(window.matchGoalsChart, teamName, matchGoalsURI, i);
  }
});

function GetMatchGoalsData(data) {
  // matchGoals[numberOfGoals] = numberOfMatches
  var matchGoals = [];
  var goals = Object.keys(data);
  var matches = Object.values(data);
  var maxGoals = goals[goals.length - 1];

  for (var i = 0; i < goals.length; i++) {
    matchGoals[parseInt(goals[i])] = parseFloat(matches[i]);
  }

  for (var i = 0; i <= maxGoals; i++) {
    goals[i] = i;
    if (matchGoals[i] == undefined)
      matchGoals[i] = 0;
  }
  chartData = new ChartData(matchGoals, goals);
  return chartData;
}

function GetMatchGoals(chart, teamName, URI, index = 0) {
  var statsRequestData = GetStatsRequestData(teamName);
  $.post(URI, statsRequestData, null, "json")
    .done(function (data) {
      var chartData = GetMatchGoalsData(data);
      if (index === 0) {
        chart.data.labels = chartData.labels;
      }
      UpdateChartData(chart, chartData.data, index)
    })
    .fail(function (jqXHR, textStatus, err) {
      console.log('Error: ' + err);
    });
}

class ChartData {
  constructor(data, labels) {
    this.data = data;
    this.labels = labels;
  }
}