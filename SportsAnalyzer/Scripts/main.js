﻿'use strict';

// chart objects
let goalsInIntervalsChart = {},
  matchGoalsChart = {},
  roundPointsChart = {};

// variables from model/API
let timeIntervalsTexts = eval($('#mainScript').attr('data-time-intervals-all-text')),
  goalsInIntervalsPercent = eval($('#mainScript').attr('data-goals-in-intervals-percent')),
  leagueName = eval($('#mainScript').attr('data-league-name')),
  seasonYear = eval($('#mainScript').attr('data-season-year')),
  teamStandings = {};

// variables from user input
let selectedRounds = [],
  firstTeam = {},
  // Order of the adding teams to the chart or presence on the chart during teams change
  teamsInSelectionOrder = {};

let chartFirstTeamName = '',
  isFirstTeamRemoved = false,
  isNewFirstTeamSelected = false;

const webApiUri = 'api/stats';

// variables for charts titles
const goalsInIntervalsTitle = 'Minutes Intervales of scored goals',
  goalsInIntervalsTooltipTitle = 'Interval',
  goalsInIntervalsURI = webApiUri + '/goalsintervals',
  goalsInIntervalsXLabel = 'Time interval [min.]',
  goalsInIntervalsYLabel = 'Percent of goals';

const matchGoalsTitle = 'Percent of Matches with a given number of goals',
  matchGoalsTooltipTitle = 'Goals',
  matchGoalsURI = webApiUri + '/matchgoals',
  matchGoalsXLabel = 'Number of goals',
  matchGoalsYLabel = 'Percent of matches';

const roundPointsLabels = Array.from(Array(20).keys(), x => x + 1),
  roundPointsTitle = 'Number of points after a given round',
  roundPointsTooltipTitle = 'Round',
  roundPointsURI = webApiUri + '/roundpoints',
  roundPointsXLabel = 'Number of round',
  roundPointsYLabel = 'Number of points';

// charts style variables
let labelsFontSize, legendFontSize, ticksFontSize, titleFontSize, tooltipsFontSize;
const alphaFactor = 0.65;
const color = Chart.helpers.color;
const myChartColors = [
  '#4DC9F6',
  '#194071',
  '#111111',
  '#FFF933',
  '#ACC236',
  '#B22222',
  '#00FA9A',
  '#FF69B4',
  '#FF5347',
  '#EEC98C',
  '#9F9F9F',
  '#F59203',
  '#808000',
];

const chartDefaultConfig = {
  responsive: true,
  maintainAspectRatio: false,
  legend: {
    position: 'top',
    labels: {
      fontSize: legendFontSize,
      boxWidth: 30,
    }
  },
  title: {
    display: true,
    fontSize: titleFontSize,
    fontColor: window.chartColors.black,
    text: 'Default Chart',
  },
  tooltips: {
    titleFontSize: tooltipsFontSize,
    bodyFontSize: tooltipsFontSize,
    mode: 'index',
  },
  scales: {
    yAxes: [{
      scaleLabel: {
        display: true,
        fontColor: window.chartColors.blue,
        fontSize: labelsFontSize,
      },
      ticks: {
        beginAtZero: true,
        fontSize: ticksFontSize,
      }
    }],
    xAxes: [{
      scaleLabel: {
        display: true,
        fontColor: window.chartColors.blue,
        fontSize: labelsFontSize,
      },
      ticks: {
        fontSize: ticksFontSize,
      }
    }]
  },
}

$('#teamsList > option').each((index, teamItem) => {
  const teamName = $(teamItem).text();
  teamStandings[teamName] = {
    points: [],
    tablePositions: [],
    opponentCrests: [],
    opposingTeams: [],
    matchResults: [],
  };
});

Chart.plugins.register({
  afterDatasetsDraw: addTablePositionsOnChart
});

function addTablePositionsOnChart(chart) {
  let ctx = chart.chart.ctx;

  // drawing the team's position in the table only on the round points chart
  if (chart.options.title.text != roundPointsTitle)
    return;

  // TODO: remove forEach and run code only for i=0 or apply .filter method
  chart.data.datasets.forEach((dataset, i) => {
    // table position will be shown only for the first team on the chart
    if (dataset.label != chartFirstTeamName)
      return;

    const teamName = dataset.label,
      meta = chart.getDatasetMeta(i);

    if (!meta.hidden) {
      meta.data.forEach((element, index) => {
        let dataString = teamStandings[teamName].tablePositions[index];

        // TODO: dataString = '-' for undefined
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

function generateStartDatasetArray(data) {
  if (Array.isArray(data)) {
    return [{
      label: leagueName,
      backgroundColor: color(myChartColors[0]).alpha(alphaFactor).rgbString(),
      borderColor: myChartColors[0],
      borderWidth: 1,
      data: data,
    }];
  }
  return [];
}

function removeChartDataset(chart, teamName) {
  chart.data.datasets = chart.data.datasets.filter(dataset => dataset.label !== teamName);
  chart.update();
}

function updateChart(chart, chartDisplaySize) {
  updateChartFontSizes(chart, chartDisplaySize);
  chart.update();
}

function confirmSelectedRounds() {
  selectedRounds = [];

  $('#roundsList :selected').each((index, roundItem) => {
    selectedRounds.push($(roundItem).val());
  });
}

function getStatsRequestData(teamName) {
  return {
    teamName,
    leagueName,
    seasonYear,
    rounds: selectedRounds,
  };
}

function addChartDataset(chart, URI, teamName, id) {
  const statsRequestData = getStatsRequestData(teamName);

  $.post(URI, statsRequestData, null, 'json')
    .done((data) => {
      let newData;
      if (URI === matchGoalsURI || URI === roundPointsURI) {
        let chartData;

        if (URI === roundPointsURI) {
          chartData = getRoundPointsData(teamName, data);
        }
        else {
          chartData = getIntegerLabeledData(data);
        }

        // update labels of chart when data for new team has more labels
        if (chartData.labels.length > chart.data.labels.length) {
          chart.data.labels = chartData.labels;
        }
        newData = chartData.data;
      }
      else {
        newData = data;
      }

      let dataset = {
        label: teamName,
        backgroundColor: color(myChartColors[id]).alpha(alphaFactor).rgbString(),
        borderColor: myChartColors[id],
        borderWidth: 1,
        data: newData,
      };

      if (URI === roundPointsURI) {
        dataset.fill = false;
        dataset.lineTension = 0;
        dataset.borderWidth = 2;
        // Add crests of opponents if dataset will be first on the chart
        if (teamName == chartFirstTeamName) {
          dataset.pointStyle = teamStandings[teamName].opponentCrests;
        }
      }

      // calling RemoveChartDataset method for case of delay in receiving results from WebApi
      removeChartDataset(chart, teamName);
      chart.data.datasets.push(dataset);
      chart.update();
    })
    .fail((jqXHR, textStatus, err) => {
      console.log('Error: ' + err);
    });
}

function getChartDisplaySize(chartName) {
  const canvasChart = document.getElementById(chartName);

  let chartDisplaySize = {
    width: parseFloat(canvasChart.style.width),
    height: parseFloat(canvasChart.style.height),
  };
  return chartDisplaySize;
}

function updateChartFontSizes(chart, chartSize) {
  if (typeof chartSize !== 'undefined') {
    legendFontSize = Math.round(0.01 * chartSize.width + 8);
    titleFontSize = Math.round(0.015 * chartSize.width + 8);
    tooltipsFontSize = Math.round(0.005 * chartSize.width + 10);
    labelsFontSize = Math.round(0.005 * chartSize.width + 10);
    ticksFontSize = Math.round(0.005 * chartSize.width + 10);
  }

  if (typeof chart !== 'undefined') {
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

function onResizeChart(chart, chartSize) {
  updateChart(chart, chartSize);
}

function createChart(chartName, title, labels, data, minY, maxY, xAxisLabel, yAxisLabel,
  typeOfChart, yAxisTicksEnding, tooltipTitlePrefix, tooltipLabelEnding) {
  const ctx = $('#' + chartName);

  let chart = new Chart(ctx, {
    // The type of chart we want to create
    type: typeOfChart,
    // The data for our dataset
    data: {
      labels: labels,
      datasets: generateStartDatasetArray(data),
    },
    // Configuration options go here
    options: chartDefaultConfig,
  });

  if (maxY != 0) {
    chart.options.scales.yAxes[0].ticks.suggestedMin = minY;
    chart.options.scales.yAxes[0].ticks.suggestedMax = maxY;
  }

  if (title == roundPointsTitle) {
    chart.options.tooltips.callbacks.label = (tooltipItem, data) => {
      const teamName = data.datasets[tooltipItem.datasetIndex].label;
      let label = teamName || '';

      if (label) {
        label += ': ';
      }
      label += tooltipItem.yLabel;
      label += tooltipLabelEnding;

      label += ' | ' + teamStandings[teamName].opposingTeams[tooltipItem.index]
        + ' ' + teamStandings[teamName].matchResults[tooltipItem.index];

      return label;
    }
  }
  else {
    chart.options.tooltips.callbacks.label = (tooltipItem, data) => {
      let label = data.datasets[tooltipItem.datasetIndex].label || '';

      if (label) {
        label += ': ';
      }
      label += tooltipItem.yLabel;
      label += tooltipLabelEnding;
      return label;
    };
  }

  chart.options.title.text = title;
  chart.options.tooltips.callbacks.title = (tooltipItems, data) => {
    const title = tooltipTitlePrefix + ': ' + tooltipItems[0].xLabel;
    return title;
  };
  chart.options.scales.yAxes[0].scaleLabel.labelString = yAxisLabel;
  chart.options.scales.xAxes[0].scaleLabel.labelString = xAxisLabel;
  chart.options.scales.yAxes[0].ticks.callback = (value, index, values) => {
    return value + yAxisTicksEnding;
  };
  chart.options.onResize = onResizeChart;

  const chartDisplaySize = getChartDisplaySize(chartName);
  updateChart(chart, chartDisplaySize);
  return chart;
}

$(document).ready(() => {
  goalsInIntervalsChart = createChart(
    'goalsInIntervalsChartArea',
    goalsInIntervalsTitle,
    timeIntervalsTexts,
    goalsInIntervalsPercent, 0, 0,
    goalsInIntervalsXLabel,
    goalsInIntervalsYLabel,
    'bar', '%',
    goalsInIntervalsTooltipTitle, '%');

  matchGoalsChart = createChart(
    'matchGoalsChartArea',
    matchGoalsTitle,
    [], [], 0, 0,
    matchGoalsXLabel,
    matchGoalsYLabel, 'bar', '%',
    matchGoalsTooltipTitle, '%');

  confirmSelectedRounds();
  getMatchGoals(matchGoalsChart, '*');

  roundPointsChart = createChart(
    'roundPointsChartArea',
    roundPointsTitle,
    roundPointsLabels, null, 0, 20,
    roundPointsXLabel,
    roundPointsYLabel, 'line', 'pts',
    roundPointsTooltipTitle, 'pts');
});

function getGoalsInIntervals(chart, index, teamName) {
  const statsRequestData = getStatsRequestData(teamName);

  $.post(goalsInIntervalsURI, statsRequestData, null, 'json')
    .done((data) => {
      updateChartData(chart, data, index);
    })
    .fail((jqXHR, textStatus, err) => {
      console.log('Error: ' + err);
    });
}

function updateChartData(chart, data, index) {
  chart.data.datasets[index].data = data;
  chart.update();
}

$('#teamsList > option').mousedown(function() {
  const teamName = $(this).text(),
    id = $(this).val();

  firstTeam = {
    name: teamName,
    id: parseInt(id),
  }
});

/** Create arrray of selected teams in order from team selected in "mousedown" event to currentTeamId*/
function createSelectedTeamsArray(currentTeamId) {
  let selectedTeams = $('#teamsList > option').filter(function(index, selectedTeam) {
    const id = index + 1;
    return id >= firstTeam.id && id <= currentTeamId;
  }).get();

  if (firstTeam.id > currentTeamId) {
    selectedTeams = $('#teamsList > option').filter(function(index) {
      const id = index + 1;
      return id >= currentTeamId && id <= firstTeam.id
    }).get();

    selectedTeams = selectedTeams.reverse();
  }
  return selectedTeams;
}

function removeTeamFromCharts(teamName) {
  removeChartDataset(goalsInIntervalsChart, teamName);
  removeChartDataset(matchGoalsChart, teamName);
  removeChartDataset(roundPointsChart, teamName);
}

function addTeamToCharts(teamName, id) {
  addChartDataset(goalsInIntervalsChart, goalsInIntervalsURI, teamName, id);
  addChartDataset(matchGoalsChart, matchGoalsURI, teamName, id);
  addChartDataset(roundPointsChart, roundPointsURI, teamName, id);
}

function changeTeamPointStyle(teamName, pointStyle) {
  const index = roundPointsChart.data.datasets.findIndex(
    dataset => dataset.label === teamName);

  if (index != -1) {
    roundPointsChart.data.datasets[index].pointStyle = pointStyle;
  }
}

$('#teamsList > option').mouseup(function() {
  const currentTeamId = parseInt($(this).val());
  const selectedTeams = createSelectedTeamsArray(currentTeamId);
  let teamsInNewOrder = Object.assign({}, teamsInSelectionOrder);

  $('#teamsList > option').each(function(index, team) {
    const teamName = $(team).text();

    if (!($(team).prop('selected'))) {
      if (teamsInSelectionOrder[teamName] == teamName) {
        delete teamsInNewOrder[teamName];
        delete teamsInSelectionOrder[teamName];
        removeTeamFromCharts(teamName);

        if (teamName === chartFirstTeamName) {
          isFirstTeamRemoved = true;
        }
      }
    }
    else if (selectedTeams.findIndex(teamItem => $(teamItem).text() === teamName) !== -1) {
      // Order from newly selected teams is prior to the existing one
      delete teamsInNewOrder[teamName];
      if (teamName === firstTeam.name && teamName !== chartFirstTeamName) {
        isNewFirstTeamSelected = true;
      }
    }
  });

  $(selectedTeams).each(function(index, selectedTeam) {
    const teamName = $(selectedTeam).text();
    const id = parseInt($(selectedTeam).val());

    if ($(selectedTeam).prop('selected')) {
      teamsInNewOrder[teamName] = teamName;
      // if there isn't the selected team on the chart yet
      if (teamsInSelectionOrder[teamName] !== teamName) {
        addTeamToCharts(teamName, id);
      }
    }
  });

  if (!isFirstTeamRemoved && isNewFirstTeamSelected) {
    // Update point style for old first team on the chart
    changeTeamPointStyle(chartFirstTeamName, 'circle');
  }
  teamsInSelectionOrder = Object.assign({}, teamsInNewOrder);

  if (isNewFirstTeamSelected || isFirstTeamRemoved) {
    for (const teamName in teamsInSelectionOrder) {
      chartFirstTeamName = teamName;
      break;
    }
    // Update point style for new first team on the chart
    changeTeamPointStyle(chartFirstTeamName, teamStandings[chartFirstTeamName].opponentCrests);
    roundPointsChart.update();
  }
  isFirstTeamRemoved = false;
  isNewFirstTeamSelected = false;
});

$('#changeRounds').click(() => {
  confirmSelectedRounds();

  goalsInIntervalsChart.data.datasets.forEach((dataset, i) => {
    let teamName = '*';

    if (dataset.label !== leagueName) {
      teamName = dataset.label;
    }
    getGoalsInIntervals(goalsInIntervalsChart, i, teamName);
    getMatchGoals(matchGoalsChart, teamName, i);
  });
});

function getRoundPointsData(teamName, data) {
  let labels = Object.keys(data).map(label => parseInt(label));
  const values = Object.values(data),
    maxRound = labels[labels.length - 1];

  values.forEach((value, i) => {
    let opponentCrest = new Image();
    opponentCrest.src = value.Opponent + '.png';
    teamStandings[teamName].points[i] = value.Points;
    teamStandings[teamName].tablePositions[i] = value.TablePosition;
    teamStandings[teamName].opponentCrests[i] = opponentCrest;
    teamStandings[teamName].opposingTeams[i] = value.OpposingTeams;
    teamStandings[teamName].matchResults[i] = value.MatchResult;
  });

  for (let round = 1; round <= maxRound; round++) {
    let i = round - 1;
    labels[i] = round;

    if (teamStandings[teamName].points[i] == undefined) {
      let blankCrest = new Image();
      blankCrest.src = 'blank.png';

      if (i == 0) {
        teamStandings[teamName].points[i] = 0;
      }
      else {
        teamStandings[teamName].points[i] = teamStandings[teamName].points[i - 1];
      }
      teamStandings[teamName].opponentCrests[i] = blankCrest;
      teamStandings[teamName].opposingTeams[i] = '';
      teamStandings[teamName].matchResults[i] = '';
    }
  }
  return new ChartData(teamStandings[teamName].points, labels);
}

function getIntegerLabeledData(data) {
  let resultArray = [],
    // add map() call to labels
    labels = Object.keys(data);
  const values = Object.values(data),
    maxLabel = labels[labels.length - 1];

  labels.forEach((label, i) => {
    resultArray[parseInt(label)] = parseFloat(values[i]);
  });

  // TODO: replace for loop with function generating array with integer values 
  //      in range <0; maxLabel>
  // filling labels with the missing ones up to maxLabel
  for (let i = 0; i <= maxLabel; i++) {
    labels[i] = i;
    if (resultArray[i] == undefined)
      resultArray[i] = 0;
  }
  return new ChartData(resultArray, labels);
}

function getMatchGoals(chart, teamName, index = 0) {
  const statsRequestData = getStatsRequestData(teamName);

  $.post(matchGoalsURI, statsRequestData, null, 'json')
    .done((data) => {
      const chartData = getIntegerLabeledData(data);
      if (index === 0) {
        chart.data.labels = chartData.labels;
      }
      updateChartData(chart, chartData.data, index)
    })
    .fail((jqXHR, textStatus, err) => {
      console.log('Error: ' + err);
    });
}

class ChartData {
  constructor(data, labels) {
    this.data = data;
    this.labels = labels;
  }
}