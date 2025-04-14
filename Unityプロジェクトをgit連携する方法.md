.gitignoreを配置

https://github.com/spoiledcat/git-for-unity を参考にgitをパッケージに追加

git for Unity の Repository Configration にリポジトリを指定する<br>
```git@github.com:LawrenceTurnerW/~~~.git```

git for Unity でmainブランチを作成しチェックアウト

ローカルにmainブランチを作成して--set-upstream-to でつなげる<br>
```git branch --set-upstream-to=origin/main main```

initした際にできる.gitignoreがコンフリクトしないようにローカルの.gitignoreを消してcommit<br>
```git rm .gitignore```<br>
```git commit -m ".gitignoreを削除"```

ローカルにマージする<br>
```git merge --allow-unrelated-histories origin/main```
