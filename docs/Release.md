# Creating a release

A release is triggered by a seperate action. This action is only triggered by creation of a tag.
It looks quite the same as the CI workflow but instead it uses the CreateRelease task and provides a repository token. This is necessary to create a github release on the repository.

It looks like this:

```
- name: Run './build.cmd CreateRelease'
  run: sudo ./build.cmd CreateRelease -GithubRepositoryAuthToken ${{ secrets.REPOSITORYTOKEN }}
```

For an actual release of a real package this would propably include pushing the created nuget packages to the nuget feed so everyone can use the packages directly from nuget.