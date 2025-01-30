using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class GameAnginAIAgent : Agent
{
    public Target[] target;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // エピソード開始時の初期化処理
    public override void OnEpisodeBegin()
    {
        // エージェントが落下したとき
        if (transform.localPosition.y < 0)
        {
            // Agentの位置と速度をリセット
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f);
        }

        // Targetの位置をランダムに決定
        for (int i = 0; i < target.Length; i++)
        {
            target[i].transform.localPosition = new Vector3(Random.value * 8 - 4, 0, Random.value * 8 - 4);
        }
    }

    // 観察値の設定
    public override void CollectObservations(VectorSensor sensor)
    {
        // Targetの位置（x,y,z)
        for (int i = 0; i < target.Length; i++)
        {
            sensor.AddObservation(target[i].transform.localPosition);
        }
        // Agentの位置（x,y,z)
        sensor.AddObservation(transform.localPosition);
        // Agentの速度（x方向)
        sensor.AddObservation(rb.velocity.x);
        // Agentの速度（z方向)
        sensor.AddObservation(rb.velocity.z);
    }

    // 行動実行時の処理
    public override void OnActionReceived(ActionBuffers actions)
    {
        // ContinuousActionsを用いて(-1.0～1.0）の範囲の連続値を取得
        var controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        rb.AddForce(controlSignal * 10);

        // 追加されたターゲットの要素数だけ
        for (int i = 0; i < target.Length; i++)
        {
            // エージェントとターゲットの距離を計算
            float distanceToTarget = Vector3.Distance(transform.localPosition, target[i].transform.localPosition);
            // エージェントとの距離が密接したとき
            if (distanceToTarget < 1.42f)
            {
                // hitしたターゲットのポイント分のプラスの報酬を与える
                AddReward(target[i].myPoint);
                // エピソードの終了
                EndEpisode();
            }
        }

        // エージェントが落下したときも
        if (this.transform.localPosition.y < -0.1f)
        {
            // エピソードを終了
            EndEpisode();
        }
    }

    // ヒューリスティックモードの行動決定時に呼ばれる
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // 矢印キーによって、移動方向を設定する
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }
}