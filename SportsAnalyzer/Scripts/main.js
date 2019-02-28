// variables from model/API
var timeIntervalsTexts = [],
  goalsInIntervalsPercent = [],
  leagueName, seasonYear,
  teamStandings = {};

// variables from user input
var selectedRounds = [],
  selectedTeams = {};

const webApiUri = 'api/stats';

// variables for charts titles
const goalsInIntervalsURI = webApiUri + "/goalsintervals",
  goalsInIntervalsTitle = "Minutes Intervales of scored goals",
  goalsInIntervalsXLabel = "Time interval [min.]",
  goalsInIntervalsYLabel = "Percent of goals",
  goalsInIntervalsTooltipTitle = "Interval";

const matchGoalsURI = webApiUri + "/matchgoals",
  matchGoalsTitle = "Percent of Matches with a given number of goals",
  matchGoalsXLabel = "Number of goals",
  matchGoalsYLabel = "Percent of matches",
  matchGoalsTooltipTitle = "Goals";

const roundPointsURI = webApiUri + "/roundpoints",
  roundPointsTitle = "Number of points after a given round",
  roundPointsXLabel = "Number of round",
  roundPointsYLabel = "Number of points",
  roundPointsTooltipTitle = "Round";

// charts style variables
var titleFontSize, ticksFontSize, legendFontSize, tooltipsFontSize, labelsFontSize;
const color = Chart.helpers.color,
  myChartColors = [
    '#4dc9f6',
    '#f67019',
    '#000000',
    '#6B8E23',
    '#acc236',
    '#B22222',
    '#00a950',
    '#58595b',
    '#FF6347',
    '#00FFFF',
    '#FF00FF',
    '#FFFF00',
    '#0000FF',
    '#00FF00',
    '#FF0000',
    '#8A2BE2',
    '#A52A2A',
    '#6495ED',
    '#BDB76B',
    '#FFA500'
  ];

const chartDefaultConfig = {
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
    fontSize: titleFontSize,
    fontColor: window.chartColors.black,
    text: 'Default Chart'
  },
  tooltips: {
    titleFontSize: tooltipsFontSize,
    bodyFontSize: tooltipsFontSize,
    mode: 'index'
  },
  scales: {
    yAxes: [{
      scaleLabel: {
        display: true,
        fontColor: window.chartColors.blue,
        fontSize: labelsFontSize
      },
      ticks: {
        beginAtZero: true,
        fontSize: ticksFontSize
      }
    }],
    xAxes: [{
      scaleLabel: {
        display: true,
        fontColor: window.chartColors.blue,
        fontSize: labelsFontSize
      },
      ticks: {
        fontSize: ticksFontSize
      }
    }]
  }
}

// Getting data of the Model passed from the Stats view

timeIntervalsTexts = eval($("#mainScript").attr("data-time-intervals-all-text"));
goalsInIntervalsPercent = eval($("#mainScript").attr("data-goals-in-intervals-percent"));
leagueName = eval($("#mainScript").attr("data-league-name"));
seasonYear = eval($("#mainScript").attr("data-season-year"));

$("#teamsList > option").each(function () {
  const teamName = $(this).text();
  selectedTeams[teamName] = false;
  teamStandings[teamName] = {
    points: [],
    tablePositions: [],
    opponentCrests: [],
    opposingTeams: [],
    matchResults: []
  };
});

Chart.plugins.register({
  afterDatasetsDraw: AddTablePositionsOnChart
});

function AddTablePositionsOnChart(chart) {
  var ctx = chart.chart.ctx;

  // drawing the team's position in the table only on the round points chart
  if (chart.options.title.text != roundPointsTitle)
    return;

  chart.data.datasets.forEach(function (dataset, i) {
    // table position will be shown only for the first team on the chart
    if (i > 0)
      return;

    const teamName = dataset.label,
      meta = chart.getDatasetMeta(i);

    if (!meta.hidden) {
      meta.data.forEach(function (element, index) {
        const numberOfRound = chart.data.labels[index];
        var dataString = teamStandings[teamName].tablePositions[numberOfRound];

        if (dataString == undefined)
          return;

        dataString = dataString.toString();
        // Draw the text in black, with the specified font
        ctx.fillStyle = 'rgb(0, 0, 0)';

        const fontSize = 14,
          fontStyle = 'normal',
          fontFamily = 'Arial';
        ctx.font = Chart.helpers.fontString(fontSize, fontStyle, fontFamily);

        // Make sure alignment settings are correct
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';

        const padding = 10,
          position = element.tooltipPosition();
        ctx.fillText(dataString, position.x, position.y - (fontSize / 2) - padding);
      });
    }
  });
}

function GenerateStartDatasetArray(data) {
  if (Array.isArray(data)) {
    return [{
      label: leagueName,
      backgroundColor: color(myChartColors[0]).alpha(0.5).rgbString(),
      borderColor: myChartColors[0],
      borderWidth: 1,
      data: data
    }];
  }
  return [];
}

function RemoveChartDataset(chart, teamName) {
  for (let i = 0; i < chart.data.datasets.length; i++) {
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
  const roundsSize = selectedRounds.length;
  for (let i = 0; i < roundsSize; i++) {
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
  const statsRequestData = GetStatsRequestData(teamName);
  $.post(URI, statsRequestData, null, "json")
    .done(function (data) {
      var newData;
      if (URI === matchGoalsURI || URI === roundPointsURI) {
        let chartData;

        if (URI === roundPointsURI) {
          chartData = GetRoundPointsData(teamName, data);
          chart.data.labels = chartData.labels;
        }
        else {
          chartData = GetIntegerLabeledData(data);
        }
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

      if (URI === roundPointsURI) {
        dataset.fill = false;
        dataset.lineTension = 0;
        dataset.borderWidth = 2;
        if (chart.data.datasets.length == 0)
          dataset.pointStyle = teamStandings[teamName].opponentCrests;
      }

      // calling RemoveChartDataset method for case of delay in receiving results from WebApi
      RemoveChartDataset(chart, teamName);
      chart.data.datasets.push(dataset);
      chart.update();
    })
    .fail(function (jqXHR, textStatus, err) {
      console.log('Error: ' + err);
    });
}

function GetChartDisplaySize(chartName) {
  const canvasChart = document.getElementById(chartName);

  var chartDisplaySize = {
    width: parseFloat(canvasChart.style.width),
    height: parseFloat(canvasChart.style.height)
  };
  return chartDisplaySize;
}

function UpdateChartFontSizes(chart, chartSize) {
  if (typeof chartSize !== "undefined") {
    legendFontSize = Math.round(0.01 * chartSize.width + 8);
    titleFontSize = Math.round(0.015 * chartSize.width + 8);
    tooltipsFontSize = Math.round(0.005 * chartSize.width + 10);
    labelsFontSize = Math.round(0.005 * chartSize.width + 10);
    ticksFontSize = Math.round(0.005 * chartSize.width + 10);
  }

  if (typeof chart !== "undefined") {
    chart.options.title.fontSize = titleFontSize;
    chart.options.legend.labels.fontSize = legendFontSize;
    chart.options.tooltips.titleFontSize = tooltipsFontSize;
    chart.options.tooltips.bodyFontSize = tooltipsFontSize;
    chart.options.scales.yAxes[0].scaleLabel.fontSize = labelsFontSize;
    chart.options.scales.xAxes[0].scaleLabel.fontSize = labelsFontSize;
    chart.options.scales.yAxes[0].ticks.fontSize = ticksFontSize;
    chart.options.scales.xAxes[0].ticks.fontSize = ticksFontSize;
  }
}

function OnResizeChart(chart, chartSize) {
  UpdateChart(chart, chartSize);
}

function CreateChart(chartName, title, labels, data, minY, maxY, xAxisLabel, yAxisLabel, typeOfChart,
  yAxisTicksEnding, tooltipTitlePrefix, tooltipLabelEnding) {
  const ctx = $("#" + chartName);
  var chart = new Chart(ctx, {
    // The type of chart we want to create
    type: typeOfChart,
    // The data for our dataset
    data: {
      labels: labels,
      datasets: GenerateStartDatasetArray(data)
    },
    // Configuration options go here
    options: chartDefaultConfig
  });

  if (maxY != 0) {
    chart.options.scales.yAxes[0].ticks.suggestedMin = minY;
    chart.options.scales.yAxes[0].ticks.suggestedMax = maxY;
  }

  if (title == roundPointsTitle) {
    chart.options.tooltips.callbacks.label = function (tooltipItem, data) {
      var label = data.datasets[tooltipItem.datasetIndex].label || '';

      if (label) {
        label += ': ';
      }
      label += tooltipItem.yLabel;
      label += tooltipLabelEnding;

      const teamName = data.datasets[tooltipItem.datasetIndex].label,
        roundNumber = parseInt(tooltipItem.xLabel);
      label += " | " + teamStandings[teamName].opposingTeams[roundNumber]
        + " " + teamStandings[teamName].matchResults[roundNumber];

      return label;
    }
  }
  else {
    chart.options.tooltips.callbacks.label = function (tooltipItem, data) {
      var label = data.datasets[tooltipItem.datasetIndex].label || '';

      if (label) {
        label += ': ';
      }
      label += tooltipItem.yLabel;
      label += tooltipLabelEnding;
      return label;
    };
  }

  chart.options.title.text = title;
  chart.options.tooltips.callbacks.title = function (tooltipItems, data) {
    const title = tooltipTitlePrefix + ": " + tooltipItems[0].xLabel;
    return title;
  };
  chart.options.scales.yAxes[0].scaleLabel.labelString = yAxisLabel;
  chart.options.scales.xAxes[0].scaleLabel.labelString = xAxisLabel;
  chart.options.scales.yAxes[0].ticks.callback = function (value, index, values) {
    return value + yAxisTicksEnding;
  };
  chart.options.onResize = OnResizeChart;

  const chartDisplaySize = GetChartDisplaySize(chartName);
  UpdateChart(chart, chartDisplaySize);
  return chart;
}

$(document).ready(function () {
  window.goalsInIntervalsChart = CreateChart(
    "goalsInIntervalsChartArea",
    goalsInIntervalsTitle,
    timeIntervalsTexts,
    goalsInIntervalsPercent,
    0, 0,
    goalsInIntervalsXLabel,
    goalsInIntervalsYLabel,
    "bar", "%", goalsInIntervalsTooltipTitle, "%");

  ConfirmSelectedRounds();
  window.matchGoalsChart = CreateChart(
    "matchGoalsChartArea",
    matchGoalsTitle,
    [],
    [],
    0, 0,
    matchGoalsXLabel,
    matchGoalsYLabel,
    "bar", "%", matchGoalsTooltipTitle, "%");

  GetMatchGoals(window.matchGoalsChart, "*");

  window.roundPointsChart = CreateChart(
    "roundPointsChartArea",
    roundPointsTitle,
    null,
    null,
    0, 20,
    roundPointsXLabel,
    roundPointsYLabel,
    "line", "pts", roundPointsTooltipTitle, "pts");
});

function GetGoalsInIntervals(chart, index, teamName) {
  var statsRequestData = GetStatsRequestData(teamName);
  $.post(goalsInIntervalsURI, statsRequestData, null, "json")
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

$("#teamsList").change(function () {
  $("#teamsList > option").each(function () {
    const teamName = $(this).text(),
      id = $(this).val();

    if ($(this).prop("selected")) {
      if (selectedTeams[teamName] === false) {
        selectedTeams[teamName] = true;
        AddChartDataset(window.goalsInIntervalsChart, goalsInIntervalsURI, teamName, id);
        AddChartDataset(window.matchGoalsChart, matchGoalsURI, teamName, id);
        AddChartDataset(window.roundPointsChart, roundPointsURI, teamName, id);
      }
    }
    else if (selectedTeams[teamName] === true) {
      selectedTeams[teamName] = false;
      RemoveChartDataset(window.goalsInIntervalsChart, teamName);
      RemoveChartDataset(window.matchGoalsChart, teamName);
      RemoveChartDataset(window.roundPointsChart, teamName);
    }
  });
});

$("#changeRounds").click(function () {
  ConfirmSelectedRounds();

  for (let i = 0; i < window.goalsInIntervalsChart.data.datasets.length; i++) {
    const dataset = window.goalsInIntervalsChart.data.datasets[i];
    var teamName = "*";

    if (dataset.label !== leagueName) {
      teamName = dataset.label;
    }

    GetGoalsInIntervals(window.goalsInIntervalsChart, i, teamName);
    GetMatchGoals(window.matchGoalsChart, teamName, i);
  }
});

function GetRoundPointsData(teamName, data) {
  var resultArray = [],
    labels = Object.keys(data);
  const values = Object.values(data),
    maxLabel = labels[labels.length - 1];

  for (let i = 0; i < labels.length; i++) {
    let opponentCrest = new Image();
    opponentCrest.src = values[i].Opponent + ".png";

    resultArray[parseInt(labels[i])] = parseInt(values[i].Points);
    teamStandings[teamName].points[parseInt(labels[i])] = values[i].Points;
    teamStandings[teamName].tablePositions[parseInt(labels[i])] = values[i].TablePosition;
    teamStandings[teamName].opponentCrests[parseInt(labels[i])] = opponentCrest;
    teamStandings[teamName].opposingTeams[parseInt(labels[i])] = values[i].OpposingTeams;
    teamStandings[teamName].matchResults[parseInt(labels[i])] = values[i].MatchResult;
  }

  for (let i = 0; i <= maxLabel; i++) {
    labels[i] = i;
    if (resultArray[i] == undefined) {
      let blankCrest = new Image();
      blankCrest.src = "blank.png";

      resultArray[i] = 0;
      teamStandings[teamName].points[i] = 0;
      teamStandings[teamName].opponentCrests[i] = blankCrest;
      teamStandings[teamName].opposingTeams[i] = "";
      teamStandings[teamName].matchResults[i] = "";
    }
  }
  return new ChartData(resultArray, labels);
}

function GetIntegerLabeledData(data) {
  var resultArray = [],
    labels = Object.keys(data);
  const values = Object.values(data),
    maxLabel = labels[labels.length - 1];

  for (let i = 0; i < labels.length; i++) {
    resultArray[parseInt(labels[i])] = parseFloat(values[i]);
  }

  for (let i = 0; i <= maxLabel; i++) {
    labels[i] = i;
    if (resultArray[i] == undefined)
      resultArray[i] = 0;
  }
  return new ChartData(resultArray, labels);
}

function GetMatchGoals(chart, teamName, index = 0) {
  const statsRequestData = GetStatsRequestData(teamName);
  $.post(matchGoalsURI, statsRequestData, null, "json")
    .done(function (data) {
      const chartData = GetIntegerLabeledData(data);
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