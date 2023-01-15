const { DataApi } = require("@unity-services/cloud-save-1.2");

const GLOBAL_PLAYER_ID = "yyZKcUEy3OhPkqMatYRM4yFL38wQ";

module.exports = async ({ params, context, logger }) => {

  const { projectId, playerId } = context;
  const { score } = params;

  const cloudSaveAPI = new DataApi(context);

  const { data } = await cloudSaveAPI.getItems(projectId, GLOBAL_PLAYER_ID, ['leaderboard']);
  let leaderboard = data.results[0].value;
  //logger.info(`leaderboard=${JSON.stringify(leaderboard, null, 2)}`);

  const playerScore = leaderboard.find(e => e.name === playerId);
  //logger.info(`playerScore ${JSON.stringify(playerScore, null, 2)}`)

  if (playerScore) {
    if (score > playerScore.score) {
      playerScore.score = score;
      //logger.info(`atualizando score para ${score}`);
     leaderboard =  await updateLeaderboard(cloudSaveAPI, projectId, leaderboard);
    } else {
      //logger.info(`mesmo score`);
    }
  } else {
    leaderboard.push({
      name: playerId,
      score,
    });
    //logger.info(`new leaderboard ${JSON.stringify(leaderboard, null, 2)}`)
    leaderboard = await updateLeaderboard(cloudSaveAPI, projectId, leaderboard);
  }

  return leaderboard;
};

const updateLeaderboard = async (cloudSaveAPI, projectId, leaderboard) => {
  // sort Array
  const sortedLeaderboard = leaderboard.sort((a, b) => b.score - a.score);
  
  await cloudSaveAPI.setItem(projectId, GLOBAL_PLAYER_ID, {
    key: "leaderboard",
    value: sortedLeaderboard
  });
  
  return sortedLeaderboard;
}

